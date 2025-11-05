// File: Controllers/ReportController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for handling obstacle reports.
/// Different roles have different access levels:
/// - Pilot & Entrepreneur: Can create and view their own reports
/// - Registrar: Can view all reports and approve/reject them
/// - Admin: Full access to everything
/// </summary>
[Authorize] // All actions require authentication
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository;
    private readonly IAdviceRepository _adviceRepository;

    public ReportController(IReportRepository reportRepository, IAdviceRepository adviceRepository)
    {
        _reportRepository = reportRepository;
        _advice_repository_fallback_check(reportRepository); // keep usage to avoid unused warning in some analyzers
        _adviceRepository = adviceRepository;
    }

    /// <summary>
    /// Displays the report creation form.
    /// Only Pilots and Entrepreneurs can create reports.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur")]
    public IActionResult Scheme()
    {
        // Get obstacle types for dropdown
        var obstacleTypes = _adviceRepository
            .GetAllObstacleTypes()
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
    /// Handles report submission.
    /// Only Pilots and Entrepreneurs can submit reports.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur")]
    public async Task<IActionResult> Scheme(Report report, string submitAction)
    {
        // Felter som settes i controller
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));
        // ModelState.Remove(nameof(Report.LastUpdated)); // bare hvis du har dette feltet

        // Hvis "Lagre kladd" – tillat uferdige felt
        if (string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
            ModelState.Remove(nameof(Report.Latitude));
            ModelState.Remove(nameof(Report.Longitude));
        }

        if (!ModelState.IsValid)
        {
            var obstacleTypes = _advice_repository_fallback();
            ViewBag.ObstacleTypes = obstacleTypes;
            return View(report);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "User not found. Please log in again.");
            var obstacleTypesError = _advice_repository_fallback();
            ViewBag.ObstacleTypes = obstacleTypesError;
            return View(report);
        }

        // Din eksisterende ID-logikk
        var lastReport = _reportRepository.GetAllReports()
            .OrderByDescending(r => r.ReportId)
            .FirstOrDefault();

        long nextId = 1000000001;
        if (lastReport != null && long.TryParse(lastReport.ReportId, out long lastId))
            nextId = lastId + 1;

        report.ReportId = nextId.ToString();
        report.UserId = userId;
        report.DateTime = DateTime.Now;

        // Sett status basert på knapp
        report.Status = string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase)
            ? "Draft"
            : "Pending";

        _reportRepository.AddReport(report);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = report.Status == "Draft"
            ? "Draft saved. You can continue later."
            : "Report submitted successfully!";

        return RedirectToAction("MyReports");

        // local helper to keep code concise
        List<SelectListItem> _advice_repository_fallback()
        {
            return _adviceRepository
                .GetAllObstacleTypes()
                .OrderBy(o => o.SortedOrder)
                .Select(o => new SelectListItem { Value = o.ObstacleId, Text = o.ObstacleName })
                .ToList();
        }
    }

    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,Registrar,Admin")]
    public IActionResult Edit(string id)
    {
        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Pilots/Entrepreneurs can only edit their own drafts
        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur"))
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
        // Registrars/Admins are allowed to edit submitted/reviewed reports (no ownership/status restriction)

        // dropdown for obstacle types
        var obstacleTypes = _adviceRepository
            .GetAllObstacleTypes()
            .OrderBy(o => o.SortedOrder)
            .Select(o => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = o.ObstacleId,
                Text = o.ObstacleName
            })
            .ToList();

        ViewBag.ObstacleTypes = obstacleTypes;
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pilot,Entrepreneur,Registrar,Admin")]
    public async Task<IActionResult> Edit(string id, Report input, string submitAction)
    {
        // Vi tillater uferdige felt ved lagring som kladd for owner roles only
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));

        var currentUserIsOwnerRole = User.IsInRole("Pilot") || User.IsInRole("Entrepreneur");
        // Only allow owners to save incomplete drafts
        if (currentUserIsOwnerRole && string.Equals(submitAction, "save", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.Remove(nameof(Report.ObstacleId));
            ModelState.Remove(nameof(Report.Description));
            ModelState.Remove(nameof(Report.Latitude));
            ModelState.Remove(nameof(Report.Longitude));
        }

        if (!ModelState.IsValid)
        {
            var obstacleTypes = _adviceRepository
                .GetAllObstacleTypes()
                .OrderBy(o => o.SortedOrder)
                .Select(o => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = o.ObstacleId,
                    Text = o.ObstacleName
                })
                .ToList();

            ViewBag.ObstacleTypes = obstacleTypes;
            return View(input);
        }

        var existing = _report_repository_getbyid(id);
        if (existing == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("MyReports");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Pilots/Entrepreneurs still must be the owner and can only edit Drafts
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
        // Registrars/Admins may edit submitted/reviewed reports without ownership/status restriction

        // Preserve previous status to decide redirect
        var previousStatus = existing.Status;

        // Update fields
        existing.Description = input.Description;
        existing.ObstacleId = input.ObstacleId;
        existing.Latitude = input.Latitude;
        existing.Longitude = input.Longitude;
        existing.AltitudeFeet = input.AltitudeFeet;
        existing.DateTime = existing.DateTime == default ? DateTime.Now : existing.DateTime; // bevar opprettet-tid

        // Only change status when the owner submits (Draft -> Pending)
        if (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            existing.Status = "Pending";

        _reportRepository.UpdateReport(existing);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = (currentUserIsOwnerRole && string.Equals(submitAction, "submit", StringComparison.OrdinalIgnoreCase))
            ? $"Report {existing.ReportId} submitted."
            : "Report updated.";

        // Redirect registrars/admins back to the appropriate admin view
        if (User.IsInRole("Registrar") || User.IsInRole("Admin"))
        {
            if (string.Equals(previousStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("PendingReports");
            return RedirectToAction("ReviewedReports");
        }

        return RedirectToAction("MyReports");

        // local helpers to keep code concise
        Report? _report_repository_getbyid(string i) => _reportRepository.GetReportById(i);

    }
    /// <summary>
    /// Displays all reports submitted by the current user.
    /// Pilots and Entrepreneurs can only see their own reports.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur")]
    public IActionResult MyReports()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User not found. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        // Filter reports by current user's Identity ID
        var reports = _reportRepository
            .GetAllReports()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.DateTime)
            .ToList();

        return View(reports);
    }

    /// <summary>
    /// Displays all pending reports for the Registrar to review.
    /// Only Registrars can access this.
    /// Supports sorting by query string: sortBy = date|user|obstacle, desc = true|false
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public IActionResult PendingReports(string sortBy = "date", bool desc = true)
    {
        // Preserve UI state for view buttons
        ViewBag.SortBy = sortBy ?? "date";
        ViewBag.Desc = desc;

        IEnumerable<Report> reports = _reportRepository
            .GetAllReports()
            .Where(r => r.Status == "Pending");

        // Apply sorting
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
    /// Displays all reviewed reports (approved and rejected).
    /// Only Registrars and Admins can access this.
    /// Now supports filtering and sorting via query-string:
    /// - filterBy: all | approved | rejected
    /// - sort: date | user | obstacle | status
    /// - desc: true | false
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public IActionResult ReviewedReports(string filterBy = "all", string sort = "date", bool desc = true)
    {
        // Preserve UI state in ViewBag for view buttons
        ViewBag.FilterBy = filterBy ?? "all";
        ViewBag.SortBy = sort ?? "date";
        ViewBag.Desc = desc;

        // Start with reviewed reports
        IEnumerable<Report> reports = _report_repository_getall_reviewed();

        // Apply filter
        if (string.Equals(filterBy, "approved", StringComparison.OrdinalIgnoreCase))
        {
            reports = reports.Where(r => r.Status == "Approved");
        }
        else if (string.Equals(filterBy, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            reports = reports.Where(r => r.Status == "Rejected");
        }

        // Apply sorting
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
    /// Approves a report.
    /// Only Registrars can approve reports.
    /// </summary>
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
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been approved.";
        return RedirectToAction("PendingReports");
    }

    /// <summary>
    /// Rejects a report.
    /// Only Registrars can reject reports.
    /// </summary>
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
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been rejected.";
        return RedirectToAction("PendingReports");
    }

    /// <summary>
    /// Displays details of a specific report.
    /// Access level depends on role:
    /// - Pilot/Entrepreneur: Can only view their own reports
    /// - Registrar/Admin: Can view all reports
    /// 
    /// Note: Registrars will be served a registrar-specific view (RegistrarDetails)
    /// so you can show different controls (Edit) there.
    /// </summary>
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

        // If user is Pilot or Entrepreneur, check if they own this report
        if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur"))
        {
            if (report.UserId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to view this report.";
                return RedirectToAction("MyReports");
            }
        }

        // If user is Registrar or Admin, show the registrar-specific details view (contains Edit button)
        if (User.IsInRole("Registrar") || User.IsInRole("Admin"))
        {
            return View("RegistrarDetails", report);
        }

        // Default: show regular details for pilots/entrepreneurs
        return View(report);
    }

    /// <summary>
    /// Deletes a report.
    /// Only Admins can delete reports.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, string? returnUrl = null)
    {
        var report = _reportRepository.GetReportById(id);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            // try to redirect back to caller (returnUrl) or referer, otherwise to AllReports
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var refererHeader = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(refererHeader) && Uri.TryCreate(refererHeader, UriKind.Absolute, out var refUri))
            {
                var localPath = refUri.PathAndQuery;
                if (Url.IsLocalUrl(localPath))
                    return Redirect(localPath);
            }

            return RedirectToAction("AllReports");
        }

        _reportRepository.DeleteReport(id);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been deleted.";

        // Prefer explicit returnUrl when provided and safe
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Fallback to Referer header (extract path + query and ensure it's local)
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
        {
            var pathAndQuery = uri.PathAndQuery;
            if (Url.IsLocalUrl(pathAndQuery))
                return Redirect(pathAndQuery);
        }

        // Default fallback
        return RedirectToAction("AllReports");
    }

    /// <summary>
    /// Displays all reports (admin overview).
    /// Only Admins can access this.
    /// Supports:
    /// - filterStatus: all|Approved|Rejected|Pending|Draft (optional)
    /// - filterId: partial report id match (optional)
    /// - sort: status|date
    /// - desc: true|false
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult AllReports(string filterStatus = "all", string filterId = "", string sort = "date", bool desc = true)
    {
        // Normalize incoming filterStatus: treat "all" or whitespace as no filter (null)
        var normalizedStatus = string.IsNullOrWhiteSpace(filterStatus) || string.Equals(filterStatus, "all", StringComparison.OrdinalIgnoreCase)
            ? null
            : filterStatus;

        // Preserve UI state (expose "all" when no status filter is applied)
        ViewBag.FilterStatus = normalizedStatus ?? "all";
        ViewBag.FilterId = filterId ?? string.Empty;
        ViewBag.SortBy = sort ?? "date";
        ViewBag.Desc = desc;

        IEnumerable<Report> reports = _reportRepository.GetAllReports();

        // Filter by partial ReportId
        if (!string.IsNullOrWhiteSpace(filterId))
        {
            var f = filterId.Trim();
            reports = reports.Where(r => !string.IsNullOrEmpty(r.ReportId) && r.ReportId.Contains(f, StringComparison.OrdinalIgnoreCase));
        }

        // Apply status filter only when a concrete status was provided
        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            reports = reports.Where(r => string.Equals(r.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase));
        }

        // Sorting (status or date)
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

    /// <summary>
    /// Update report status (Registrar/Admin).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registrar,Admin")]
    public async Task<IActionResult> UpdateStatus(string id, string newStatus, string? returnUrl = null)
    {
        var allowed = new[] { "Pending", "Approved", "Rejected", "Draft" };

        if (string.IsNullOrWhiteSpace(newStatus) || !allowed.Any(s => string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["ErrorMessage"] = "Invalid status.";
            return RedirectToAction("Details", new { id });
        }

        var report = _reportRepository.GetReportById(id);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("PendingReports");
        }

        // Normalize to canonical casing from allowed list
        report.Status = allowed.First(s => string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase));
        report.LastUpdated = DateTime.Now;
        _reportRepository.UpdateReport(report);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} status changed to {report.Status}.";

        // Prefer explicit returnUrl when provided and safe
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Fallback to Referer header (extract path + query and ensure it's local)
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
        {
            var pathAndQuery = uri.PathAndQuery;
            if (Url.IsLocalUrl(pathAndQuery))
                return Redirect(pathAndQuery);
        }

        return RedirectToAction("ReviewedReports");
    }

    // small local helpers used above to keep main methods tidy
    IEnumerable<Report> _report_repository_getall_reviewed() =>
        _reportRepository.GetAllReports().Where(r => r.Status == "Approved" || r.Status == "Rejected");

    void _advice_repository_fallback_check(IReportRepository _) { /* no-op to avoid unused param warnings in some analyzers */ }
}