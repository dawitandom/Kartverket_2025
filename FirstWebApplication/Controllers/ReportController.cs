using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using FirstWebApplication.DataContext;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace FirstWebApplication.Controllers;

[Authorize] // Krever innlogging for alle actions i denne controlleren
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository; // Repo-lag for rapporter (CRUD)
    private readonly ApplicationContext _db;              // EF Core DbContext for oppslag (ObstacleTypes)

    // Dependency Injection: får inn repository og DbContext fra containeren
    public ReportController(IReportRepository reportRepository, ApplicationContext db)
    {
        _reportRepository = reportRepository;
        _db = db;
    }

    // Slår opp alle roller per bruker via Identity-tabellene og returnerer som dictionary.
    // Krever at ApplicationContext arver fra IdentityDbContext slik at _db.UserRoles og _db.Roles finnes.
    private Dictionary<string, List<string>> GetUserRolesLookup()
    {
        var rolesLookup =
            (from ur in _db.UserRoles
             join r in _db.Roles on ur.RoleId equals r.Id
             group r.Name by ur.UserId into g
             select new { UserId = g.Key, Roles = g.ToList() })
            .ToDictionary(x => x.UserId, x => x.Roles);

        return rolesLookup;
    }
    
    private Dictionary<string, List<string>> GetUserOrganizationsLookup()
    {
        var orgLookup =
            (from ou in _db.OrganizationUsers
                join o in _db.Organizations on ou.OrganizationId equals o.OrganizationId
                group o.ShortCode by ou.UserId into g
                select new { UserId = g.Key, Orgs = g.ToList() })
            .ToDictionary(x => x.UserId, x => x.Orgs);

        return orgLookup;
    }


    // GET: /Report/Scheme
    // Viser skjema for å opprette en ny rapport (kun Pilot/Entrepreneur/DefaultUser).
    // Fyller ViewBag.ObstacleTypes med nedtrekksvalg fra databasen.
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public IActionResult Scheme()
    {
        var obstacleTypes = _db.ObstacleTypes
            .OrderBy(o => o.SortedOrder)
            .Select(o => new SelectListItem
            {
                Value = o.ObstacleId,
                Text = o.ObstacleName   
            })
            .ToList();

        ViewBag.ObstacleTypes = obstacleTypes;
        return View();
    }

    // POST: /Report/Scheme
    // - "save": lagrer som Draft -> krever bare lokasjon
    // - "submit": krever alle felter (lokasjon + obstacle + description)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public async Task<IActionResult> Scheme(
        [Bind(
            nameof(Report.Latitude),
            nameof(Report.Longitude),
            nameof(Report.Geometry),
            nameof(Report.HeightFeet),
            nameof(Report.ObstacleId),
            nameof(Report.Description),
            nameof(Report.Geometry))]
        Report report,
        string submitAction)
    {
        // Felter som settes i controller
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));

        var isSave = string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase);
        var isSubmit = string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase);

        // SAVE: vi vil IKKE kreve Obstacle/Description – fjern dem fra validering
        if (isSave)
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
            // HeightFeet lar vi stå – validering (range) kjøres bare hvis den har verdi
        }

        // Både SAVE og SUBMIT krever posisjon
        if ((isSave || isSubmit) && (report.Latitude is null || report.Longitude is null))
        {
            ModelState.AddModelError(string.Empty, "Location is required.");
        }

        // SUBMIT: alle felter skal være påkrevd
        if (isSubmit)
        {
            // Obstacle må være satt
            if (string.IsNullOrWhiteSpace(report.ObstacleId))
            {
                ModelState.AddModelError(nameof(Report.ObstacleId),
                    "Obstacle type is required when submitting.");
            }

            // Description må være satt og minst 10 tegn
            if (string.IsNullOrWhiteSpace(report.Description))
            {
                ModelState.AddModelError(nameof(Report.Description),
                    "Description is required when submitting.");
            }
            else if (report.Description.Trim().Length < 10)
            {
                ModelState.AddModelError(nameof(Report.Description),
                    "Description must be at least 10 characters.");
            }

            // NEW: height påkrevd ved submit
            if (report.HeightFeet is null)
            {
                ModelState.AddModelError(nameof(Report.HeightFeet),
                    "Height is required when submitting.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ObstacleTypes = _db.ObstacleTypes
                .OrderBy(o => o.SortedOrder)
                .Select(o => new SelectListItem { Value = o.ObstacleId, Text = o.ObstacleName })
                .ToList();

            return View(report);
        }

        // Finn innlogget bruker
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "User not found. Please log in again.");

            ViewBag.ObstacleTypes = _db.ObstacleTypes
                .OrderBy(o => o.SortedOrder)
                .Select(o => new SelectListItem { Value = o.ObstacleId, Text = o.ObstacleName })
                .ToList();

            return View(report);
        }

        report.UserId = userId;
        report.DateTime = DateTime.Now;
        report.Status = isSave ? "Draft" : "Pending";

        await _reportRepository.AddAsync(report);

        TempData["SuccessMessage"] = isSave
            ? "Draft saved. You can continue later."
            : "Report submitted successfully!";

        return RedirectToAction("MyReports");
    }




    // GET: /Report/Edit/{id}
    // Viser redigeringsskjema for en rapport:
    // - Pilot/Entrepreneur/DefaultUser: kan bare redigere egne rapporter, og kun hvis status = Draft
    // - Registrar/Admin: kan redigere innsendte/ferdige rapporter (f.eks. korrigering)
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser,Registrar,Admin")]
    public IActionResult Edit(string id)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Eier-regler for Pilot/Entrepreneur/DefaultUser:
        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser"))
        {
            if (report.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this report.";
                return RedirectToAction("MyReports");
            }

            if (!string.Equals(report.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only drafts can be edited by the report owner.";
                return RedirectToAction("MyReports");
            }
        }

        // Dropdown for obstacle typer
        var obstacleTypes = _db.ObstacleTypes
            .OrderBy(o => o.SortedOrder)
            .Select(o => new SelectListItem
            {
                Value = o.ObstacleId,
                Text = o.ObstacleName
            })
            .ToList();

        ViewBag.ObstacleTypes = obstacleTypes;
        return View(report);
    }

    // POST: /Report/Edit/{id}
    // Oppdaterer en rapport:
    // - Eier kan lagre som kladd (tillater uferdig) eller sende inn (Draft -> Pending)
    // - Registrar/Admin kan oppdatere felt (f.eks. opprydding før godkjenning)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser,Registrar,Admin")]
    public async Task<IActionResult> Edit(string id, Report input, string submitAction)
    {
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));

        var currentUserIsOwnerRole =
            User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser");

        var isSave = string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase);
        var isSubmit = string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase);

        // Eier som lagrer kladd: obstacle + description kan være tomme
        if (currentUserIsOwnerRole && isSave)
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
        }

        // Eier må alltid ha posisjon (både save og submit)
        if (currentUserIsOwnerRole && (isSave || isSubmit) &&
            (input.Latitude is null || input.Longitude is null))
        {
            ModelState.AddModelError("", "Location is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ObstacleTypes = _db.ObstacleTypes
                .OrderBy(o => o.SortedOrder)
                .Select(o => new SelectListItem
                {
                    Value = o.ObstacleId,
                    Text = o.ObstacleName
                })
                .ToList();

            return View(input);
        }

        var existing = _reportRepository.GetReportById(id);
        if (existing == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Eier-regler: må eie rapporten og status må være Draft
        if (currentUserIsOwnerRole)
        {
            if (existing.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this report.";
                return RedirectToAction("MyReports");
            }

            if (!string.Equals(existing.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only drafts can be edited by the report owner.";
                return RedirectToAction("MyReports");
            }
        }

        var previousStatus = existing.Status;

        // Oppdater redigerbare felter
        existing.Description = input.Description;
        existing.ObstacleId = input.ObstacleId;
        existing.Latitude = input.Latitude;
        existing.Longitude = input.Longitude;
        existing.HeightFeet = input.HeightFeet;
        existing.Geometry = input.Geometry;
        // Bevar opprettelses-tid, sett hvis tom
        existing.DateTime = existing.DateTime == default ? DateTime.Now : existing.DateTime;

        // Eier som trykker "submit" endrer status fra Draft -> Pending
        if (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            existing.Status = "Pending";

        _reportRepository.UpdateReport(existing);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            ? $"Report {existing.ReportId} submitted."
            : "Report updated.";

        // Registrar/Admin sendes tilbake til riktig oversikt basert på tidligere status
        if (User.IsInRole("Registrar") || User.IsInRole("Admin"))
        {
            if (string.Equals(previousStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("PendingReports");
            return RedirectToAction("ReviewedReports");
        }

        // Eier tilbake til egen liste
        return RedirectToAction("MyReports");
    }

    // GET: /Report/MyReports
    // Viser alle rapporter som tilhører innlogget Pilot/Entrepreneur/DefaultUser (sortert nyest først)
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public IActionResult MyReports()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User not found. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        var reports = _reportRepository
            .GetAllReports()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.DateTime)
            .ToList();

        return View(reports);
    }

    // GET: /Report/PendingReports
    // Viser alle rapporter med status "Pending" for Registrar/Admin.
    // Støtter sortering via query: sortBy = date|user|obstacle, desc = true|false
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public IActionResult PendingReports(string sortBy = "date", bool desc = true)
    {
        ViewBag.SortBy = sortBy ?? "date";
        ViewBag.Desc = desc;
        ViewBag.UserRoles = GetUserRolesLookup();
        ViewBag.UserOrganizations = GetUserOrganizationsLookup();

        IEnumerable<Report> reports = _reportRepository
            .GetAllReports()
            .Where(r => r.Status == "Pending");

        switch ((sortBy ?? "date").ToLowerInvariant())
        {
            case "user":
                reports = desc
                    ? reports.OrderByDescending(r => r.User?.UserName ?? string.Empty)
                    : reports.OrderBy(r => r.User?.UserName ?? string.Empty);
                break;
            case "obstacle":
                reports = desc
                    ? reports.OrderByDescending(r => r.ObstacleType?.ObstacleName ?? string.Empty)
                    : reports.OrderBy(r => r.ObstacleType?.ObstacleName ?? string.Empty);
                break;
            case "date":
            default:
                reports = desc
                    ? reports.OrderByDescending(r => r.DateTime)
                    : reports.OrderBy(r => r.DateTime);
                break;
        }

        return View(reports.ToList());
    }

    // GET: /Report/ReviewedReports
    // Viser "Approved" og "Rejected" for Registrar/Admin.
    // Støtter filter (approved|rejected|all), organisasjonsfilter (org short code or 'all') og sortering (date|user|obstacle|status).
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public IActionResult ReviewedReports(string filterBy = "all", string sort = "date", bool desc = true, string org = "all")
    {
        ViewBag.FilterBy = filterBy ?? "all";
        ViewBag.SortBy = sort ?? "date";
        ViewBag.Desc = desc;
        ViewBag.FilterOrg = org ?? "all";
        ViewBag.UserRoles = GetUserRolesLookup();
        ViewBag.UserOrganizations = GetUserOrganizationsLookup();

        // Provide full organization list for UI
        ViewBag.Organizations = _db.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new SelectListItem { Value = o.ShortCode, Text = o.Name })
            .ToList();

        IEnumerable<Report> reports = _reportRepository
            .GetAllReports()
            .Where(r => r.Status == "Approved" || r.Status == "Rejected");

        if (string.Equals(filterBy, "approved", StringComparison.OrdinalIgnoreCase))
            reports = reports.Where(r => r.Status == "Approved");
        else if (string.Equals(filterBy, "rejected", StringComparison.OrdinalIgnoreCase))
            reports = reports.Where(r => r.Status == "Rejected");

        // Organization filter: org is expected to be Organization.ShortCode or "all"
        if (!string.IsNullOrWhiteSpace(org) && !string.Equals(org, "all", StringComparison.OrdinalIgnoreCase))
        {
            var selectedShortCode = org.Trim();
            var userIds = (from ou in _db.OrganizationUsers
                           join o in _db.Organizations on ou.OrganizationId equals o.OrganizationId
                           where o.ShortCode == selectedShortCode
                           select ou.UserId).Distinct().ToList();

            if (userIds.Any())
            {
                reports = reports.Where(r => userIds.Contains(r.UserId));
            }
            else
            {
                // No users in that org -> empty result
                reports = Enumerable.Empty<Report>();
            }
        }

        switch ((sort ?? "date").ToLowerInvariant())
        {
            case "user":
                reports = desc
                    ? reports.OrderByDescending(r => r.User?.UserName ?? string.Empty)
                    : reports.OrderBy(r => r.User?.UserName ?? string.Empty);
                break;
            case "obstacle":
                reports = desc
                    ? reports.OrderByDescending(r => r.ObstacleType?.ObstacleName ?? string.Empty)
                    : reports.OrderBy(r => r.ObstacleType?.ObstacleName ?? string.Empty);
                break;
            case "status":
                reports = desc
                    ? reports.OrderByDescending(r => r.Status ?? string.Empty)
                    : reports.OrderBy(r => r.Status ?? string.Empty);
                break;
            case "date":
            default:
                reports = desc
                    ? reports.OrderByDescending(r => r.DateTime)
                    : reports.OrderBy(r => r.DateTime);
                break;
        }

        return View(reports.ToList());
    }

    // POST: /Report/Approve
    // Godkjenner en rapport (Registrar/Admin).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> Approve(string id)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("PendingReports");
        }

        report.Status = "Approved";
        _reportRepository.UpdateReport(report);

        // Notification
        _db.Notifications.Add(new Notification
        {
            UserId = report.UserId,
            ReportId = report.ReportId,
            Title = "Report approved",
            Message = $"Your report {report.ReportId} was approved.",
            CreatedAt = DateTime.Now,
            IsRead = false
        });

        await _reportRepository.SaveChangesAsync();
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been approved.";
        return RedirectToAction("PendingReports");
    }

    // POST: /Report/Reject
    // Avviser en rapport (Registrar/Admin).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> Reject(string id)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("PendingReports");
        }

        report.Status = "Rejected";
        _reportRepository.UpdateReport(report);

        // Notification
        _db.Notifications.Add(new Notification
        {
            UserId = report.UserId,
            ReportId = report.ReportId,
            Title = "Report rejected",
            Message = $"Your report {report.ReportId} was rejected.",
            CreatedAt = DateTime.Now,
            IsRead = false
        });

        await _reportRepository.SaveChangesAsync();
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been rejected.";
        return RedirectToAction("PendingReports");
    }

    // POST: /Report/UpdateStatus
    // Generic status updater used by Registrar/Admin from RegistrarDetails view.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        string newStatus,
        string? registrarComment = null,
        string? returnUrl = null)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("PendingReports");
        }

        var previousStatus = report.Status ?? string.Empty;

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            TempData["ErrorMessage"] = "No status selected.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("ReviewedReports");
        }

        // Apply status change
        report.Status = newStatus;
        report.LastUpdated = DateTime.Now;

        // Oppdater kommentar hvis satt
        if (!string.IsNullOrWhiteSpace(registrarComment))
        {
            report.RegistrarComment = registrarComment;
        }

        // lag notification ved Approved / Rejected
        if (!string.Equals(previousStatus, newStatus, StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(newStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(newStatus, "Rejected", StringComparison.OrdinalIgnoreCase)))
        {
            var title = string.Equals(newStatus, "Approved", StringComparison.OrdinalIgnoreCase)
                ? "Report approved"
                : "Report rejected";

            var baseMessage = $"Your report {report.ReportId} was {newStatus.ToLowerInvariant()}.";
            var fullMessage = string.IsNullOrWhiteSpace(report.RegistrarComment)
                ? baseMessage
                : $"{baseMessage} Comment: {report.RegistrarComment}";

            _db.Notifications.Add(new Notification
            {
                UserId = report.UserId,
                ReportId = report.ReportId,
                Title = title,
                Message = fullMessage,
                CreatedAt = DateTime.Now,
                IsRead = false
            });
        }

        _reportRepository.UpdateReport(report);
        await _reportRepository.SaveChangesAsync();
        await _db.SaveChangesAsync(); // kan droppes om du heller vil bruke bare repo-context

        TempData["SuccessMessage"] = $"Report {id} status changed to {newStatus}.";

        // Prefer a provided safe returnUrl
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Otherwise route based on previous status to keep UX consistent
        if (string.Equals(previousStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("PendingReports");

        return RedirectToAction("ReviewedReports");
    }


    // GET: /Report/Details/{id}
    // Viser detaljer:
    // - Eier (Pilot/Entrepreneur/DefaultUser) kan bare se egne rapporter
    // - Registrar/Admin kan se alle og får egen view ("RegistrarDetails")
    [HttpGet]
    public IActionResult Details(string id)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser"))
        {
            if (report.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to view this report.";
                return RedirectToAction("MyReports");
            }
        }

        if (User.IsInRole("Registrar") || User.IsInRole("Admin"))
            return View("RegistrarDetails", report);

        return View(report);
    }

    // POST: /Report/Delete
    // Sletter en rapport (kun Admin). Har litt navigasjonslogikk for å sende bruker tilbake
    // til siden de kom fra (returnUrl eller HTTP Referer) eller til AllReports som fallback.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser,Registrar,Admin")]
    public async Task<IActionResult> Delete(string id, string? returnUrl = null)
    {
        var report = _reportRepository.GetReportById(id);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // --- ADMIN: kan slette alt ---
        if (User.IsInRole("Admin"))
        {
            _reportRepository.DeleteReport(id);
            await _reportRepository.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Report {id} deleted.";
            return Redirect(returnUrl ?? "/Report/AllReports");
        }

        // --- PILOT / ENTREPRENEUR / DEFAULTUSER: kun egne rapporter ---
        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser"))
        {
            // Må eie rapporten
            if (report.UserId != userId)
            {
                TempData["ErrorMessage"] = "You do not own this report.";
                return RedirectToAction("MyReports");
            }

            // Kan bare slette Draft + Pending
            if (report.Status != "Draft" && report.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only Draft or Pending reports can be deleted.";
                return RedirectToAction("MyReports");
            }

            _reportRepository.DeleteReport(id);
            await _reportRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Report {id} deleted.";
            return RedirectToAction("MyReports");
        }

        // Andre roller skal ikke slette
        return Forbid();
    }

    // GET: /Report/AllReports
    // Admin-oversikt over alle rapporter, med filtrering og sortering.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult AllReports(string filterStatus = "all", string filterId = "", string sort = "date", bool desc = true)
    {
        // Normaliser "all" til null for enklere filterlogikk
        var normalizedStatus = string.IsNullOrWhiteSpace(filterStatus) || string.Equals(filterStatus, "all", StringComparison.OrdinalIgnoreCase)
            ? null
            : filterStatus;

        // ViewBag-verdier for å holde på UI-state
        ViewBag.FilterStatus = normalizedStatus ?? "all";
        ViewBag.FilterId = filterId ?? string.Empty;
        ViewBag.SortBy = sort ?? "date";
        ViewBag.Desc = desc;
        ViewBag.UserRoles = GetUserRolesLookup();

        IEnumerable<Report> reports = _reportRepository.GetAllReports();

        // Fritekstsøk på ReportId (delstreng)
        if (!string.IsNullOrWhiteSpace(filterId))
        {
            var f = filterId.Trim();
            reports = reports.Where(r => !string.IsNullOrEmpty(r.ReportId) && r.ReportId.Contains(f, StringComparison.OrdinalIgnoreCase));
        }

        // Status-filter hvis spesifisert
        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            reports = reports.Where(r => string.Equals(r.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase));
        }

        // Sorter på status eller dato
        switch ((sort ?? "date").ToLowerInvariant())
        {
            case "status":
                reports = desc
                    ? reports.OrderByDescending(r => r.Status ?? string.Empty)
                    : reports.OrderBy(r => r.Status ?? string.Empty);
                break;

            case "date":
            default:
                reports = desc
                    ? reports.OrderByDescending(r => r.DateTime)
                    : reports.OrderBy(r => r.DateTime);
                break;
        }

        return View(reports.ToList());
    }
}
