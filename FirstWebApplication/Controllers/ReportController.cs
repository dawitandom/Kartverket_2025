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

/// <summary>
/// Controller for håndtering av rapporter i systemet. Lar brukere opprette, redigere, se og slette rapporter
/// avhengig av deres rolle. Pilot, Entrepreneur og DefaultUser kan opprette og administrere egne rapporter.
/// Registrar og Admin kan se alle rapporter, godkjenne eller avvise dem, og redigere dem.
/// </summary>
[Authorize] // Krever innlogging for alle actions i denne controlleren
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ApplicationContext _db; // For oppslag av ObstacleTypes og roller

    /// <summary>
    /// Oppretter en ny instans av ReportController med de angitte tjenestene.
    /// </summary>
    /// <param name="reportRepository">Repository for rapportdata</param>
    /// <param name="notificationRepository">Repository for varsler</param>
    /// <param name="organizationRepository">Repository for organisasjonsdata</param>
    /// <param name="db">Databasekontekst for å hente hindertyper og roller</param>
    public ReportController(
        IReportRepository reportRepository,
        INotificationRepository notificationRepository,
        IOrganizationRepository organizationRepository,
        ApplicationContext db)
    {
        _reportRepository = reportRepository;
        _notificationRepository = notificationRepository;
        _organizationRepository = organizationRepository;
        _db = db;
    }

    /// <summary>
    /// Henter alle roller for alle brukere fra Identity-tabellene og returnerer dem som en ordbok.
    /// Dette brukes for å vise brukerroller i rapporter uten å måtte gjøre separate database-spørringer for hver bruker.
    /// </summary>
    /// <returns>En ordbok hvor nøkkelen er bruker-ID og verdien er en liste over rollenavn</returns>
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
    
    /// <summary>
    /// Henter alle organisasjoner for alle brukere og returnerer dem som en ordbok.
    /// Dette brukes for å vise organisasjoner i rapporter uten å måtte gjøre separate database-spørringer for hver bruker.
    /// </summary>
    /// <returns>En ordbok hvor nøkkelen er bruker-ID og verdien er en liste over organisasjonsnavn</returns>
    private async Task<Dictionary<string, List<string>>> GetUserOrganizationsLookupAsync()
    {
        return await _organizationRepository.GetUserOrganizationLookupAsync();
    }


    /// <summary>
    /// Viser skjemaet for å opprette en ny rapport. Kun tilgjengelig for Pilot, Entrepreneur og DefaultUser.
    /// Skjemaet inneholder felter for posisjon, hindertype, høyde og beskrivelse.
    /// Henter alle tilgjengelige hindertyper fra databasen og legger dem i ViewBag for nedtrekkslisten.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public IActionResult Create()
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

    /// <summary>
    /// Håndterer opprettelsen av en ny rapport. Støtter to handlinger:
    /// - "save": Lagrer rapporten som kladd (Draft) - krever kun posisjon
    /// - "submit": Sender inn rapporten (Pending) - krever alle felter (posisjon, hindertype, høyde og beskrivelse)
    /// Hvis valideringen feiler, vises skjemaet på nytt med feilmeldinger.
    /// </summary>
    /// <param name="report">Rapportobjektet med informasjonen som skal lagres</param>
    /// <param name="submitAction">Handlingen som skal utføres: "save" for kladd eller "submit" for innsending</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public async Task<IActionResult> Create(
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
        // Fjerner felter som settes automatisk i controller fra validering
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));

        var isSave = string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase);
        var isSubmit = string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase);

        // Ved lagring som kladd: Obstacle og Description er ikke påkrevd
        if (isSave)
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
            // HeightFeet valideres kun hvis den har verdi (range-validering)
        }

        // Både lagring og innsending krever posisjon
        if ((isSave || isSubmit) && (report.Latitude is null || report.Longitude is null))
        {
            ModelState.AddModelError(string.Empty, "Location is required.");
        }

        // Ved innsending: alle felter må være utfylt
        if (isSubmit)
        {
            // Hindertype må være valgt
            if (string.IsNullOrWhiteSpace(report.ObstacleId))
            {
                ModelState.AddModelError(nameof(Report.ObstacleId),
                    "Obstacle type is required when submitting.");
            }

            // Beskrivelse må være utfylt og minst 10 tegn
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

            // Høyde er påkrevd ved innsending
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

        // Henter ID for innlogget bruker
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




    /// <summary>
    /// Viser redigeringsskjemaet for en eksisterende rapport. Tilgangsregler:
    /// - Pilot/Entrepreneur/DefaultUser: Kan bare redigere egne rapporter, og kun hvis status er Draft eller Pending
    /// - Registrar/Admin: Kan redigere alle rapporter uavhengig av status (for eksempel for korrigeringer)
    /// Henter hindertyper fra databasen og legger dem i ViewBag for nedtrekkslisten.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal redigeres</param>
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser,Registrar,Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Sjekker tilgangsrettigheter for Pilot/Entrepreneur/DefaultUser
        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser"))
        {
            if (report.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this report.";
                return RedirectToAction("MyReports");
            }

            // Eiere kan kun redigere Draft eller Pending rapporter
            var editableStatuses = new[] { "Draft", "Pending" };
            if (!editableStatuses.Contains(report.Status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only Draft or Pending reports can be edited by the report owner.";
                return RedirectToAction("MyReports");
            }
        }

        // Henter hindertyper for nedtrekksliste
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


    /// <summary>
    /// Oppdaterer en eksisterende rapport basert på informasjonen i skjemaet. Støtter to handlinger for eiere:
    /// - "save": Lagrer endringene som kladd (tillater uferdige felter)
    /// - "submit": Sender inn rapporten (endrer status fra Draft til Pending, krever alle felter)
    /// Registrar og Admin kan oppdatere alle felter uavhengig av status.
    /// Sjekker tilgangsrettigheter før oppdateringen utføres.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal oppdateres</param>
    /// <param name="input">Rapportobjektet med de oppdaterte verdiene</param>
    /// <param name="submitAction">Handlingen som skal utføres: "save" for kladd eller "submit" for innsending</param>
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

        // Ved lagring som kladd: Obstacle og Description er ikke påkrevd
        if (currentUserIsOwnerRole && isSave)
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
        }

        // Posisjon er alltid påkrevd (både ved lagring og innsending)
        if (currentUserIsOwnerRole && (isSave || isSubmit) &&
            (input.Latitude is null || input.Longitude is null))
        {
            ModelState.AddModelError("", "Location is required.");
        }
        
        // Ved innsending: alle felter må være utfylt (samme validering som ved opprettelse)
        if (currentUserIsOwnerRole && isSubmit)
        {
            // Hindertype må være valgt
            if (string.IsNullOrWhiteSpace(input.ObstacleId))
            {
                ModelState.AddModelError(nameof(Report.ObstacleId),
                    "Obstacle type is required when submitting.");
            }

            // Beskrivelse må være utfylt og minst 10 tegn
            if (string.IsNullOrWhiteSpace(input.Description))
            {
                ModelState.AddModelError(nameof(Report.Description),
                    "Description is required when submitting.");
            }
            else if (input.Description.Trim().Length < 10)
            {
                ModelState.AddModelError(nameof(Report.Description),
                    "Description must be at least 10 characters.");
            }

            // Høyde er påkrevd ved innsending
            if (input.HeightFeet is null)
            {
                ModelState.AddModelError(nameof(Report.HeightFeet),
                    "Height is required when submitting.");
            }
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

        var existing = await _reportRepository.GetByIdAsync(id);
        if (existing == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Sjekker tilgangsrettigheter: eiere må eie rapporten og status må være Draft eller Pending
        if (currentUserIsOwnerRole)
        {
            if (existing.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this report.";
                return RedirectToAction("MyReports");
            }

            var editableStatuses = new[] { "Draft", "Pending" };
            if (!editableStatuses.Contains(existing.Status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only Draft or Pending reports can be edited by the report owner.";
                return RedirectToAction("MyReports");
            }
        }

        var previousStatus = existing.Status;

        // Oppdaterer alle redigerbare felter fra input
        existing.Description = input.Description;
        existing.ObstacleId = input.ObstacleId;
        existing.Latitude = input.Latitude;
        existing.Longitude = input.Longitude;
        existing.HeightFeet = input.HeightFeet;
        existing.Geometry = input.Geometry;
        // Bevarer opprettelsestidspunkt, setter kun hvis det er tomt
        existing.DateTime = existing.DateTime == default ? DateTime.Now : existing.DateTime;

        // Ved innsending endres status fra Draft til Pending
        if (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            existing.Status = "Pending";

        await _reportRepository.UpdateAsync(existing);

        TempData["SuccessMessage"] = (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            ? $"Report {existing.ReportId} submitted."
            : "Report updated.";

        // Sender brukeren tilbake til detaljvisning
        // Registrar/Admin får RegistrarDetails-view, eiere får vanlig Details-view
        return RedirectToAction("Details", new { id = existing.ReportId });
    }

    /// <summary>
    /// Viser alle rapporter som tilhører den innloggede brukeren. Kun tilgjengelig for Pilot, Entrepreneur og DefaultUser.
    /// Rapporter vises sortert med nyeste først, slik at brukeren enkelt kan se sine siste rapporter.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public async Task<IActionResult> MyReports()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User not found. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        var reports = await _reportRepository.GetByUserIdAsync(userId);
        return View(reports);
    }

    /// <summary>
    /// Viser alle rapporter med status "Pending" som venter på gjennomgang. Kun tilgjengelig for Registrar og Admin.
    /// Støtter sortering på dato, bruker eller hindertype, og filtrering på organisasjon.
    /// Brukes av registratorer og administratorer for å se hvilke rapporter som trenger gjennomgang.
    /// </summary>
    /// <param name="sortBy">Hvilket felt som skal brukes for sortering: "date", "user" eller "obstacle"</param>
    /// <param name="desc">Om sorteringen skal være synkende (true) eller stigende (false)</param>
    /// <param name="org">Organisasjonsfilter for å begrense hvilke organisasjoners rapporter som vises ("all" for alle)</param>
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> PendingReports(string sortBy = "date", bool desc = true, string org = "all")
    {
        ViewBag.SortBy = sortBy ?? "date";
        ViewBag.Desc = desc;
        ViewBag.UserRoles = GetUserRolesLookup();
        ViewBag.UserOrganizations = await GetUserOrganizationsLookupAsync();
        ViewBag.FilterOrg = org ?? "all";

        // Tilby full organisasjonsliste for UI (brukes av filter-popover)
        var organizations = await _organizationRepository.GetAllAsync();
        ViewBag.Organizations = organizations
            .Select(o => new SelectListItem { Value = o.ShortCode, Text = o.Name })
            .ToList();

        var allReports = await _reportRepository.GetAllAsync();
        IEnumerable<Report> reports = allReports.Where(r => r.Status == "Pending");

        // Organisasjonsfilter: org forventes å være Organization.ShortCode eller "all"
        if (!string.IsNullOrWhiteSpace(org) && !string.Equals(org, "all", StringComparison.OrdinalIgnoreCase))
        {
            var selectedShortCode = org.Trim();
            var userIds = await _organizationRepository.GetUserIdsForOrganizationShortCodeAsync(selectedShortCode);

            if (userIds.Any())
            {
                reports = reports.Where(r => userIds.Contains(r.UserId));
            }
            else
            {
                // Ingen brukere i den organisasjonen -> tomt resultat
                reports = Enumerable.Empty<Report>();
            }
        }

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

    /// <summary>
    /// Viser alle rapporter som har blitt gjennomgått, det vil si rapporter med status "Approved" eller "Rejected".
    /// Kun tilgjengelig for Registrar og Admin. Støtter filtrering på godkjent/avvist/alle,
    /// organisasjonsfilter og sortering på dato, bruker, hindertype eller status.
    /// </summary>
    /// <param name="filterBy">Filter for å vise kun godkjente ("approved"), avviste ("rejected") eller alle ("all")</param>
    /// <param name="sort">Hvilket felt som skal brukes for sortering: "date", "user", "obstacle" eller "status"</param>
    /// <param name="desc">Om sorteringen skal være synkende (true) eller stigende (false)</param>
    /// <param name="org">Organisasjonsfilter for å begrense hvilke organisasjoners rapporter som vises ("all" for alle)</param>
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> ReviewedReports(string filterBy = "all", string sort = "date", bool desc = true, string org = "all")
    {
        ViewBag.FilterBy = filterBy ?? "all";
        ViewBag.SortBy = sort ?? "date";
        ViewBag.Desc = desc;
        ViewBag.FilterOrg = org ?? "all";
        ViewBag.UserRoles = GetUserRolesLookup();
        ViewBag.UserOrganizations = await GetUserOrganizationsLookupAsync();

        // Tilby full organisasjonsliste for UI
        var organizations = await _organizationRepository.GetAllAsync();
        ViewBag.Organizations = organizations
            .Select(o => new SelectListItem { Value = o.ShortCode, Text = o.Name })
            .ToList();

        var allReports = await _reportRepository.GetAllAsync();
        IEnumerable<Report> reports = allReports.Where(r => r.Status == "Approved" || r.Status == "Rejected");

        if (string.Equals(filterBy, "approved", StringComparison.OrdinalIgnoreCase))
            reports = reports.Where(r => r.Status == "Approved");
        else if (string.Equals(filterBy, "rejected", StringComparison.OrdinalIgnoreCase))
            reports = reports.Where(r => r.Status == "Rejected");

        // Organisasjonsfilter: org forventes å være Organization.ShortCode eller "all"
        if (!string.IsNullOrWhiteSpace(org) && !string.Equals(org, "all", StringComparison.OrdinalIgnoreCase))
        {
            var selectedShortCode = org.Trim();
            var userIds = await _organizationRepository.GetUserIdsForOrganizationShortCodeAsync(selectedShortCode);

            if (userIds.Any())
            {
                reports = reports.Where(r => userIds.Contains(r.UserId));
            }
            else
            {
                // Ingen brukere i den organisasjonen -> tomt resultat
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

    /// <summary>
    /// Godkjenner en rapport ved å endre statusen til "Approved". Kun tilgjengelig for Registrar og Admin.
    /// Oppretter automatisk et varsel til rapportens eier om at rapporten er godkjent.
    /// Sender brukeren tilbake til listen over ventende rapporter etter godkjenningen.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal godkjennes</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> Approve(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("PendingReports");
        }

        report.Status = "Approved";
        await _reportRepository.UpdateAsync(report);

        // Opprett varsel via repository
        await _notificationRepository.CreateForReportStatusChangeAsync(
            report.UserId,
            report.ReportId,
            "Report approved",
            $"Your report {report.ReportId} was approved.");

        TempData["SuccessMessage"] = $"Report {id} has been approved.";
        return RedirectToAction("PendingReports");
    }

    /// <summary>
    /// Avviser en rapport ved å endre statusen til "Rejected". Kun tilgjengelig for Registrar og Admin.
    /// Oppretter automatisk et varsel til rapportens eier om at rapporten er avvist.
    /// Sender brukeren tilbake til listen over ventende rapporter etter avvisningen.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal avvises</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> Reject(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("PendingReports");
        }

        report.Status = "Rejected";
        await _reportRepository.UpdateAsync(report);

        // Opprett varsel via repository
        await _notificationRepository.CreateForReportStatusChangeAsync(
            report.UserId,
            report.ReportId,
            "Report rejected",
            $"Your report {report.ReportId} was rejected.");

        TempData["SuccessMessage"] = $"Report {id} has been rejected.";
        return RedirectToAction("PendingReports");
    }

    /// <summary>
    /// Oppdaterer statusen på en rapport til en ny verdi. Kun tilgjengelig for Registrar og Admin.
    /// Brukes fra detaljvisningen for å endre status direkte. Støtter også å legge til en kommentar fra registratoren.
    /// Oppretter automatisk varsler til rapportens eier når status endres til "Approved" eller "Rejected".
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal oppdateres</param>
    /// <param name="newStatus">Den nye statusen som skal settes på rapporten</param>
    /// <param name="registrarComment">Valgfri kommentar fra registratoren som legges til rapporten</param>
    /// <param name="returnUrl">Valgfri URL som brukeren skal sendes til etter oppdateringen</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        string newStatus,
        string? registrarComment = null,
        string? returnUrl = null)
    {
        var report = await _reportRepository.GetByIdAsync(id);
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

        // Bruk statusendring
        report.Status = newStatus;
        report.LastUpdated = DateTime.Now;

        // Oppdater kommentar hvis angitt
        if (!string.IsNullOrWhiteSpace(registrarComment))
        {
            report.RegistrarComment = registrarComment;
        }

        await _reportRepository.UpdateAsync(report);

        // Opprett varsel for godkjent/avvist statusendringer
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

            await _notificationRepository.CreateForReportStatusChangeAsync(
                report.UserId,
                report.ReportId,
                title,
                fullMessage);
        }

        TempData["SuccessMessage"] = $"Report {id} status changed to {newStatus}.";

        // Foretrekk en angitt sikker returnUrl
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Ellers ruter basert på forrige status for å holde UX konsistent
        if (string.Equals(previousStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("PendingReports");

        return RedirectToAction("ReviewedReports");
    }


    /// <summary>
    /// Viser detaljene for en spesifikk rapport. Tilgangsregler:
    /// - Eiere (Pilot/Entrepreneur/DefaultUser) kan bare se egne rapporter
    /// - Registrar og Admin kan se alle rapporter og får en egen detaljvisning med ekstra funksjoner
    /// Hvis rapporten ikke finnes eller brukeren ikke har tilgang, vises en feilmelding.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal vises</param>
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
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

    /// <summary>
    /// Sletter en rapport fra systemet. Tilgangsregler:
    /// - Admin kan slette alle rapporter
    /// - Eiere (Pilot/Entrepreneur/DefaultUser) kan bare slette egne rapporter, og kun hvis status er Draft eller Pending
    /// Sjekker tilgangsrettigheter før slettingen utføres. Sender brukeren tilbake til returnUrl hvis angitt,
    /// ellers til en passende side basert på brukerens rolle.
    /// </summary>
    /// <param name="id">ID-en til rapporten som skal slettes</param>
    /// <param name="returnUrl">Valgfri URL som brukeren skal sendes til etter slettingen</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser,Registrar,Admin")]
    public async Task<IActionResult> Delete(string id, string? returnUrl = null)
    {
        var report = await _reportRepository.GetByIdAsync(id);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // --- ADMIN: kan slette alt ---
        if (User.IsInRole("Admin"))
        {
            await _reportRepository.DeleteAsync(id);
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

            await _reportRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Report {id} deleted.";
            return RedirectToAction("MyReports");
        }

        // Andre roller skal ikke slette
        return Forbid();
    }

    /// <summary>
    /// Viser en oversikt over alle rapporter i systemet. Kun tilgjengelig for Admin.
    /// Støtter filtrering på status og søk på rapport-ID, samt sortering på status eller dato.
    /// Brukes av administratorer for å få en fullstendig oversikt over alle rapporter i systemet.
    /// </summary>
    /// <param name="filterStatus">Statusfilter for å begrense hvilke rapporter som vises ("all" for alle statuser)</param>
    /// <param name="filterId">Søkefilter for å finne rapporter med en spesifikk ID eller delstreng av ID</param>
    /// <param name="sort">Hvilket felt som skal brukes for sortering: "status" eller "date"</param>
    /// <param name="desc">Om sorteringen skal være synkende (true) eller stigende (false)</param>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllReports(string filterStatus = "all", string filterId = "", string sort = "date", bool desc = true)
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

        var allReports = await _reportRepository.GetAllAsync();
        IEnumerable<Report> reports = allReports;

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
