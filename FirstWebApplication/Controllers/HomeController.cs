using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for the home page.
    /// Shows role-specific dashboard with quick actions.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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

                    // Preferred: if the admin's username equals the organization's ShortCode, use that org.
                    if (!string.IsNullOrWhiteSpace(user.UserName))
                    {
                        orgName = await _db.Organizations
                            .Where(o => o.ShortCode == user.UserName)
                            .Select(o => o.Name)
                            .FirstOrDefaultAsync();
                    }

                    // Fallback: find first organization linked via OrganizationUsers
                    if (orgName == null)
                    {
                        orgName = await _db.OrganizationUsers
                            .Where(ou => ou.UserId == user.Id)
                            .Include(ou => ou.Organization)
                            .Select(ou => ou.Organization!.Name)
                            .FirstOrDefaultAsync();
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