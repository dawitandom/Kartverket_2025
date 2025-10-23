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
    public async Task<IActionResult> Scheme(Report report)
    {
        // Remove validation errors for fields that will be set by controller
        ModelState.Remove(nameof(Report.ReportId));
        ModelState.Remove(nameof(Report.UserId));
        ModelState.Remove(nameof(Report.DateTime));
        ModelState.Remove(nameof(Report.Status));

        if (ModelState.IsValid)
        {
            // Get the current user's ID from Identity (AspNetUsers.Id)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User not found. Please log in again.");

                // Reload obstacle types for dropdown
                var obstacleTypesError = _adviceRepository
                    .GetAllObstacleTypes()
                    .OrderBy(o => o.SortedOrder)
                    .Select(o => new SelectListItem
                    {
                        Value = o.ObstacleId,
                        Text = o.ObstacleName
                    })
                    .ToList();

                ViewBag.ObstacleTypes = obstacleTypesError;
                return View(report);
            }

            // Generate unique 10-character report ID (format: yyMMddHHmm)
            report.ReportId = DateTime.Now.ToString("yyMMddHHmm");

            // Set user ID from Identity (this now works with AspNetUsers)
            report.UserId = userId;
            report.DateTime = DateTime.Now;
            report.Status = "Pending"; // Default status

            // Save report to database
            _reportRepository.AddReport(report);
            await _reportRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report submitted successfully!";
            return RedirectToAction("MyReports");
        }

        // If model is invalid, reload obstacle types for dropdown
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
        return View(report);
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
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Registrar,Admin")]
    public IActionResult PendingReports()
    {
        var pendingReports = _reportRepository
            .GetAllReports()
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.DateTime)
            .ToList();

        return View(pendingReports);
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
        IEnumerable<Report> reports = _reportRepository
            .GetAllReports()
            .Where(r => r.Status == "Approved" || r.Status == "Rejected");

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

        return View(report);
    }

    /// <summary>
    /// Deletes a report.
    /// Only Admins can delete reports.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var report = _reportRepository.GetReportById(id);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction("ReviewedReports");
        }

        _reportRepository.DeleteReport(id);
        await _reportRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Report {id} has been deleted.";
        return RedirectToAction("ReviewedReports");
    }

    /// <summary>
    /// Displays all reports (admin overview).
    /// Only Admins can access this.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult AllReports()
    {
        var allReports = _reportRepository
            .GetAllReports()
            .OrderByDescending(r => r.DateTime)
            .ToList();

        return View(allReports);
    }
}
