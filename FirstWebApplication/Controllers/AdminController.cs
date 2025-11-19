using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using FirstWebApplication.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FirstWebApplication.DataContext;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for admin user management.
/// Only accessible by users with Admin role.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationContext _db;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    /// <summary>
    /// Displays all users in the system with their roles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = _userManager.Users.ToList();

        // Slå opp hvilke organisasjoner hver bruker tilhører (OrganizationUsers + Organizations)
        var orgLookup =
            (from ou in _db.OrganizationUsers
                join o in _db.Organizations on ou.OrganizationId equals o.OrganizationId
                group o.ShortCode by ou.UserId into g
                select new { UserId = g.Key, Orgs = g.ToList() })
            .ToDictionary(x => x.UserId, x => x.Orgs);

        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // Finn alle org-shortcodes for denne brukeren (hvis noen)
            orgLookup.TryGetValue(user.Id, out var orgsForUser);

            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                Organizations = orgsForUser ?? new List<string>()
            });
        }

        return View(userViewModels);
    }


    /// <summary>
    /// Displays the create user form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        // Get all roles for dropdown
        var roles = _roleManager.Roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        ViewBag.Roles = roles;
        return View();
    }

    /// <summary>
    /// Handles user creation.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to selected role
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                TempData["SuccessMessage"] = $"User {model.UserName} created successfully!";
                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Reload roles for dropdown
        var roles = _roleManager.Roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        ViewBag.Roles = roles;
        return View(model);
    }

    /// <summary>
    /// Deletes a user from the system.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("ManageUsers");
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User {user.UserName} deleted successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete user.";
        }

        return RedirectToAction("ManageUsers");
    }
}