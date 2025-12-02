using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for organisasjonsadministratorer.
/// Kun tilgjengelig for brukere med OrgAdmin-rollen. Lar organisasjonsadministratorer
/// administrere medlemmer i sin organisasjon og se rapporter fra organisasjonens brukere.
/// </summary>
[Authorize(Roles = "OrgAdmin")]
public class OrgAdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationContext _db;
    private readonly IOrganizationRepository _organizationRepository;

    /// <summary>
    /// Oppretter en ny instans av OrgAdminController med de angitte tjenestene.
    /// </summary>
    /// <param name="db">Databasekontekst for å hente rapporter</param>
    /// <param name="userManager">UserManager for å administrere brukere</param>
    /// <param name="organizationRepository">Repository for organisasjonsdata</param>
    public OrgAdminController(
        ApplicationContext db,
        UserManager<ApplicationUser> userManager,
        IOrganizationRepository organizationRepository)
    {
        _userManager = userManager;
        _db = db;
        _organizationRepository = organizationRepository;
    }

    /// <summary>
    /// Finner organisasjons-ID-en for den nåværende organisasjonsadministratoren.
    /// Først sjekkes om brukernavnet matcher en organisasjons shortcode, ellers brukes den første
    /// organisasjonslenken via OrganizationUser-tabellen.
    /// </summary>
    /// <returns>Organisasjons-ID-en hvis funnet, ellers null</returns>
    private async Task<int?> GetCurrentOrgIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;

        return await _organizationRepository.ResolveOrgIdForAdminAsync(user.Id, user.UserName);
    }

    // ========== 1) Styre hvilke brukere som hører til organisasjonen ==========

    /// <summary>
    /// Viser en liste over alle medlemmer i organisasjonen som organisasjonsadministratoren tilhører.
    /// For hvert medlem vises brukernavn, e-post, fullt navn og tilknyttede roller.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Members()
    {
        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        var org = await _organizationRepository.GetByIdAsync(orgId.Value);
        if (org == null) return NotFound();

        var members = await _organizationRepository.GetMembersWithUserAsync(orgId.Value);

        var memberDtos = new List<OrgMemberDto>();

        foreach (var m in members)
        {
            if (m.User == null) continue;

            var roles = await _userManager.GetRolesAsync(m.User);
            var rolesDisplay = roles != null && roles.Any()
                ? string.Join(", ", roles)
                : "—";

            memberDtos.Add(new OrgMemberDto
            {
                UserId = m.UserId,
                UserName = m.User.UserName ?? "",
                Email = m.User.Email ?? "",
                FullName = $"{m.User.FirstName} {m.User.LastName}",
                Roles = rolesDisplay
            });
        }

        var model = new OrgMembersViewModel
        {
            OrganizationId = org.OrganizationId,
            OrganizationName = org.Name,
            Members = memberDtos
        };

        return View(model);
    }


    /// <summary>
    /// Legger til en eksisterende bruker i organisasjonen ved å søke etter brukeren med brukernavn eller e-post.
    /// Hvis brukeren ikke finnes eller allerede er medlem, vises en passende melding.
    /// Ved vellykket tillegg sendes en bekreftelsesmelding.
    /// </summary>
    /// <param name="userNameOrEmail">Brukernavnet eller e-postadressen til brukeren som skal legges til</param>
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

        if (!await _organizationRepository.MemberExistsAsync(orgId.Value, user.Id))
        {
            await _organizationRepository.AddMemberAsync(orgId.Value, user.Id);
            TempData["Success"] = $"User '{user.UserName}' was added to the organization.";
        }
        else
        {
            TempData["Info"] = $"User '{user.UserName}' is already a member of this organization.";
        }

        return RedirectToAction(nameof(Members));
    }

    /// <summary>
    /// Fjerner en bruker fra organisasjonen. Sjekker først at brukeren faktisk er medlem av organisasjonen
    /// før fjerningen utføres. Viser en feilmelding hvis brukeren ikke er medlem.
    /// </summary>
    /// <param name="userId">ID-en til brukeren som skal fjernes fra organisasjonen</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(string userId)
    {
        var orgId = await GetCurrentOrgIdAsync();
        if (orgId == null) return Forbid();

        if (!await _organizationRepository.MemberExistsAsync(orgId.Value, userId))
        {
            TempData["Error"] = "User is not a member of this organization.";
            return RedirectToAction(nameof(Members));
        }

        await _organizationRepository.RemoveMemberAsync(orgId.Value, userId);
        TempData["Success"] = "User removed from organization.";
        return RedirectToAction(nameof(Members));
    }

    // ========== 2) Se alle rapporter sendt inn av brukere i organisasjonen ==========

    /// <summary>
    /// Viser alle rapporter som er sendt inn av brukere i organisasjonen som organisasjonsadministratoren tilhører.
    /// Støtter filtrering på status og brukernavn, samt sortering på status eller dato.
    /// Hvis organisasjonsadministratoren ikke har en spesifikk organisasjon, vises rapporter fra alle organisasjoner.
    /// </summary>
    /// <param name="filterStatus">Statusfilter for å begrense hvilke rapporter som vises (f.eks. "Pending", "Approved")</param>
    /// <param name="filterUser">Brukernavnfilter for å søke etter rapporter fra en spesifikk bruker</param>
    /// <param name="filterId">Alternativt filter-ID (brukes hvis filterUser ikke er angitt)</param>
    /// <param name="sort">Hvilket felt som skal brukes for sortering ("status" eller "date")</param>
    /// <param name="desc">Om sorteringen skal være synkende (true) eller stigende (false)</param>
    [HttpGet]
    public async Task<IActionResult> OrgReports(string? filterStatus, string? filterUser, string? filterId, string? sort, bool? desc)
    {
        var orgId = await GetCurrentOrgIdAsync();

        // keep backward compatibility: if filterUser not provided, use filterId
        if (string.IsNullOrWhiteSpace(filterUser) && !string.IsNullOrWhiteSpace(filterId))
        {
            filterUser = filterId;
        }

        // Base query: include user and obstacle type
        var query = _db.Reports
            .Include(r => r.User)
            .Include(r => r.ObstacleType)
            .AsQueryable();

        // Limit to this org (or any organization if admin has no specific org)
        if (orgId != null)
        {
            query = query.Where(r => r.User != null &&
                                     r.User.Organizations.Any(o => o.OrganizationId == orgId.Value));
        }
        else
        {
            // Admin has no specific organization link: return reports associated with any organization
            query = query.Where(r => r.User != null && r.User.Organizations.Any());
        }

        // Apply Reporter username filter (partial match)
        if (!string.IsNullOrWhiteSpace(filterUser))
        {
            var userFilter = filterUser.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.User != null &&
                r.User.UserName != null &&
                EF.Functions.Like(r.User.UserName.ToLower(), $"%{userFilter}%"));
        }

        // Apply status filter (if provided and not "all")
        if (!string.IsNullOrWhiteSpace(filterStatus) && !filterStatus.Equals("all", System.StringComparison.OrdinalIgnoreCase))
        {
            var statusLower = filterStatus.Trim().ToLowerInvariant();
            query = query.Where(r => r.Status != null && r.Status.ToLower() == statusLower);
        }

        // Sorting
        var sortKey = string.IsNullOrWhiteSpace(sort) ? "date" : sort.Trim().ToLowerInvariant();
        var isDesc = desc ?? true;

        switch (sortKey)
        {
            case "status":
                query = isDesc
                    ? query.OrderByDescending(r => r.Status).ThenByDescending(r => r.DateTime)
                    : query.OrderBy(r => r.Status).ThenByDescending(r => r.DateTime);
                break;

            case "date":
            default:
                query = isDesc
                    ? query.OrderByDescending(r => r.DateTime)
                    : query.OrderBy(r => r.DateTime);
                break;
        }

        var reports = await query.ToListAsync();

        // Preserve UI state for the view
        ViewBag.FilterStatus = string.IsNullOrWhiteSpace(filterStatus) ? "all" : filterStatus;
        ViewBag.FilterUser = filterUser ?? string.Empty;
        ViewBag.SortBy = sortKey;
        ViewBag.Desc = isDesc;

        return View("OrgReports", reports);
    }
}
