using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for administrasjon av alle organisasjoner i systemet.
/// Kun tilgjengelig for brukere med Admin-rollen. Lar administratorer se alle organisasjoner,
/// opprette nye organisasjoner og opprette organisasjonsadministratorer for hver organisasjon.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrganizationsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationsController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOrganizationRepository organizationRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _organizationRepository = organizationRepository;
    }

    /// <summary>
    /// Viser en liste over alle organisasjoner i systemet med deres navn, shortcode og annen relevant informasjon.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orgs = await _organizationRepository.GetAllAsync();
        return View(orgs);
    }

    /// <summary>
    /// Viser skjemaet for å opprette en ny organisasjon. Skjemaet inneholder felter for organisasjonsnavn,
    /// shortcode og annen relevant informasjon om organisasjonen.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Organization());
    }

    /// <summary>
    /// Håndterer opprettelsen av en ny organisasjon basert på informasjonen i skjemaet.
    /// Validerer at all nødvendig informasjon er oppgitt før organisasjonen opprettes.
    /// Viser en bekreftelsesmelding ved vellykket opprettelse.
    /// </summary>
    /// <param name="model">Modellen som inneholder informasjonen om den nye organisasjonen</param>
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

    /// <summary>
    /// Viser skjemaet for å opprette en organisasjonsadministrator for en spesifikk organisasjon.
    /// Skjemaet inneholder felter for brukernavn, e-post, navn og passord for den nye organisasjonsadministratoren.
    /// </summary>
    /// <param name="id">ID-en til organisasjonen som skal få en ny organisasjonsadministrator</param>
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

    /// <summary>
    /// Håndterer opprettelsen av en organisasjonsadministrator for en organisasjon.
    /// Oppretter brukeren med OrgAdmin-rollen og knytter dem til organisasjonen.
    /// Hvis opprettelsen lykkes, sendes en bekreftelsesmelding og brukeren sendes tilbake til organisasjonslisten.
    /// </summary>
    /// <param name="model">Modellen som inneholder informasjonen om den nye organisasjonsadministratoren</param>
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

