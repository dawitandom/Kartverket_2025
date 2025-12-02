using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for hjemmesiden i applikasjonen.
    /// Viser en rollebasert dashboard med rasktilgang til ulike funksjoner avhengig av brukerens rolle.
    /// For eksempel vises forskjellige handlinger for piloter, registratorer, administratorer og organisasjonsadministratorer.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrganizationRepository _organizationRepository;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IOrganizationRepository organizationRepository)
        {
            _userManager = userManager;
            _organizationRepository = organizationRepository;
        }

        /// <summary>
        /// Viser hjemmesiden med rollebasert innhold og rasktilgang:
        /// - Pilot/Entrepreneur/DefaultUser: Rasktilgang til å opprette og se egne rapporter
        /// - Registrar: Rasktilgang til ventende og gjennomgåtte rapporter
        /// - Admin: Rasktilgang til brukeradministrasjon
        /// - OrgAdmin: Viser organisasjonsnavn og rasktilgang til organisasjonsspesifikke funksjoner
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // If the current user is an OrgAdmin, try to resolve the organization name and
            // a friendly display name for the user. The view will prefer OrganizationName.
            if (User?.Identity?.IsAuthenticated == true && User.IsInRole("OrgAdmin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    string? orgName = null;

                    // Preferred: if username matches org short code
                    if (!string.IsNullOrWhiteSpace(user.UserName))
                    {
                        var orgByShortCode = await _organizationRepository.GetByShortCodeAsync(user.UserName);
                        orgName = orgByShortCode?.Name;
                    }

                    // Fallback: first organization linked via OrganizationUsers
                    if (orgName == null)
                    {
                        orgName = await _organizationRepository.GetFirstOrganizationNameForUserAsync(user.Id);
                    }

                    ViewBag.OrganizationName = orgName; // may be null — view handles fallback
                    ViewBag.UserDisplayName = string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? user.UserName
                        : $"{user.FirstName} {user.LastName}";
                }
            }

            return View();
        }

        /// <summary>
        /// Viser informasjonssiden om applikasjonen. Alle kan nå denne siden, også uten å være innlogget.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult About()
        {
            return View(); // Leter etter Views/Home/About.cshtml
        }
    }
}