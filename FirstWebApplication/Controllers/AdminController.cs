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
/// Controller for administrasjon av brukere i systemet.
/// Kun tilgjengelig for brukere med Admin-rollen. Lar administratorer se alle brukere,
/// opprette nye brukere med ulike roller, og slette brukere.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;

    /// <summary>
    /// Oppretter en ny instans av AdminController med de angitte tjenestene.
    /// </summary>
    /// <param name="userManager">UserManager for å administrere brukere</param>
    /// <param name="roleManager">RoleManager for å administrere roller</param>
    /// <param name="organizationRepository">Repository for organisasjonsdata</param>
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
    /// Viser en liste over alle brukere i systemet med deres roller og organisasjoner.
    /// Hver bruker vises med brukernavn, e-post, navn, tilknyttede roller og hvilke organisasjoner de tilhører.
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
    /// Viser skjemaet for å opprette en ny bruker. Skjemaet inneholder felter for brukernavn,
    /// e-post, navn, passord og valg av rolle. Alle tilgjengelige roller vises i en nedtrekksliste.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        // Henter alle roller for nedtrekksliste
        var roles = _roleManager.Roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        ViewBag.Roles = roles;
        return View();
    }

    /// <summary>
    /// Håndterer opprettelsen av en ny bruker basert på informasjonen i skjemaet.
    /// Oppretter brukeren med angitt rolle og sender en bekreftelsesmelding ved vellykket opprettelse.
    /// Hvis opprettelsen feiler, vises feilmeldinger og skjemaet vises på nytt.
    /// </summary>
    /// <param name="model">Modellen som inneholder informasjonen om den nye brukeren</param>
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
                // Legg brukeren til valgt rolle
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

        // Last inn roller på nytt for nedtrekksliste
        var roles = _roleManager.Roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        ViewBag.Roles = roles;
        return View(model);
    }

    /// <summary>
    /// Sletter en bruker fra systemet basert på brukerens ID.
    /// Hvis brukeren ikke finnes eller slettingen feiler, vises en feilmelding.
    /// Ved vellykket sletting sendes en bekreftelsesmelding og brukeren sendes tilbake til brukerlisten.
    /// </summary>
    /// <param name="id">ID-en til brukeren som skal slettes</param>
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