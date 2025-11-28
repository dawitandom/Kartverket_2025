using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers;

[Authorize(Roles = "Admin")]
public class OrganizationAdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationAdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationRepository organizationRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationRepository = organizationRepository;
    }

    // 1) Liste over organisasjoner
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orgs = await _organizationRepository.GetAllAsync();

        return View(orgs); // -> Views/OrganizationAdmin/Index.cshtml
    }

    // 2) Opprett ny organisasjon
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Organization());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Organization model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _organizationRepository.AddAsync(model);

        TempData["Success"] = "Organization created.";
        return RedirectToAction(nameof(Index));
    }

    // 3) Opprett OrgAdmin-bruker for en organisasjon
    [HttpGet]
    public async Task<IActionResult> CreateOrgAdmin(int id)
    {
        var org = await _organizationRepository.GetByIdAsync(id);
        if (org == null) return NotFound();

        var vm = new CreateOrgAdminViewModel
        {
            OrganizationId = org.OrganizationId,
            OrganizationName = org.Name
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrgAdmin(CreateOrgAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var org = await _organizationRepository.GetByIdAsync(model.OrganizationId);
        if (org == null)
        {
            ModelState.AddModelError(string.Empty, "Organization not found.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var e in createResult.Errors)
            {
                ModelState.AddModelError("", e.Description);
            }
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync("OrgAdmin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("OrgAdmin"));
        }

        await _userManager.AddToRoleAsync(user, "OrgAdmin");

        await _organizationRepository.AddMemberAsync(org.OrganizationId, user.Id);

        TempData["Success"] = $"Org admin '{user.UserName}' created for {org.Name}.";
        return RedirectToAction(nameof(Index));
    }
}
