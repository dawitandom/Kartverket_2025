using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for the home page.
    /// Shows role-specific dashboard with quick actions.
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
        /// Displays role-specific home page:
        /// - Pilot/Entrepreneur: Quick actions for creating and viewing reports
        /// - Registrar: Quick actions for pending and reviewed reports
        /// - Admin: Quick actions for user management
        /// - OrgAdmin: sets ViewBag.OrganizationName (used to show "Welcome, {org} admin!")
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

        // 👇 Denne skal du lime inn her — bare denne!
        [HttpGet]
        [AllowAnonymous]
        public IActionResult About()
        {
            return View(); // Leter etter Views/Home/About.cshtml
        }
    }
}