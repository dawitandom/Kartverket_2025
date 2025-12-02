using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for å administrere brukere.
/// Bare admin kan bruke denne.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;

    /// <summary>
    /// Lager en ny AdminController.
    /// </summary>
    /// <param name="userManager">Håndterer brukere.</param>
    /// <param name="roleManager">Håndterer roller.</param>
    /// <param name="organizationRepository">Håndterer organisasjoner.</param>
    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationRepository organizationRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationRepository = organizationRepository;
    }

    /// <summary>
    /// Viser alle brukere og deres roller.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = _userManager.Users.ToList();

        // Slå opp hvilke organisasjoner hver bruker tilhører (OrganizationUsers + Organizations)
        var orgLookup = await _organizationRepository.GetUserOrganizationLookupAsync();

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
    /// Viser skjema for å lage ny bruker.
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
    /// Håndterer oppretting av ny bruker.
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
    /// Sletter en bruker.
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