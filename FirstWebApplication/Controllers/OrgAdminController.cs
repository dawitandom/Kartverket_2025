using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers;

[Authorize(Roles = "OrgAdmin")]
public class OrgAdminController : Controller
{
    private readonly ApplicationContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrgAdminController(ApplicationContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>
    /// Finds the OrganizationId for the current OrgAdmin (assumes one org).
    /// </summary>
    private async Task<int?> GetCurrentOrgIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        return await _db.OrganizationUsers
            .Where(ou => ou.UserId == userId)
            .Select(ou => (int?)ou.OrganizationId)
            .FirstOrDefaultAsync();
    }

    // ========== 1) Styre hvilke brukere som hører til organisasjonen ==========

    [HttpGet]
    public async Task<IActionResult> Members()
    {
        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        var org = await _db.Organizations.FindAsync(orgId.Value);
        if (org == null) return NotFound();

        var members = await _db.OrganizationUsers
            .Where(ou => ou.OrganizationId == orgId.Value)
            .Include(ou => ou.User)
            .OrderBy(ou => ou.User!.UserName)
            .ToListAsync();

        var model = new OrgMembersViewModel
        {
            OrganizationId = org.OrganizationId,
            OrganizationName = org.Name,
            Members = members.Select(m => new OrgMemberDto
            {
                UserId = m.UserId,
                UserName = m.User!.UserName ?? "",
                Email = m.User!.Email ?? "",
                FullName = $"{m.User!.FirstName} {m.User!.LastName}"
            }).ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Add an existing user (by username or email) to this OrgAdmin's organization.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(string userNameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            TempData["Error"] = "You must enter a username or email.";
            return RedirectToAction(nameof(Members));
        }

        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        var user =
            await _userManager.FindByNameAsync(userNameOrEmail)
            ?? await _userManager.FindByEmailAsync(userNameOrEmail);

        if (user == null)
        {
            TempData["Error"] = $"No user found with '{userNameOrEmail}'.";
            return RedirectToAction(nameof(Members));
        }

        var exists = await _db.OrganizationUsers
            .AnyAsync(ou => ou.OrganizationId == orgId.Value && ou.UserId == user.Id);

        if (!exists)
        {
            _db.OrganizationUsers.Add(new OrganizationUser
            {
                OrganizationId = orgId.Value,
                UserId = user.Id
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"User '{user.UserName}' was added to the organization.";
        }
        else
        {
            TempData["Info"] = $"User '{user.UserName}' is already a member of this organization.";
        }

        return RedirectToAction(nameof(Members));
    }

    /// <summary>
    /// Remove a user from this OrgAdmin's organization.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(string userId)
    {
        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        var link = await _db.OrganizationUsers
            .FirstOrDefaultAsync(ou => ou.OrganizationId == orgId.Value && ou.UserId == userId);

        if (link == null)
        {
            TempData["Error"] = "User is not a member of this organization.";
            return RedirectToAction(nameof(Members));
        }

        _db.OrganizationUsers.Remove(link);
        await _db.SaveChangesAsync();

        TempData["Success"] = "User removed from organization.";
        return RedirectToAction(nameof(Members));
    }

    // ========== 2) Se alle rapporter sendt inn av brukere i organisasjonen ==========

    [HttpGet]
    public async Task<IActionResult> OrgReports()
    {
        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        var reports = await _db.Reports
            .Include(r => r.User)
            .Include(r => r.ObstacleType)
            .Where(r => r.User != null &&
                        r.User.Organizations.Any(o => o.OrganizationId == orgId.Value))
            .OrderByDescending(r => r.DateTime)
            .ToListAsync();
        
        return View("OrgReports", reports);
    }
}
