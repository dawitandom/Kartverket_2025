using System.Linq;
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
    /// Controller for brukerprofil.
    /// Lar brukere se sin profil og slette kontoen.
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;

        /// <summary>
        /// Lager en ny ProfileController.
        /// </summary>
        /// <param name="userManager">Håndterer brukere.</param>
        /// <param name="signInManager">Håndterer innlogging.</param>
        /// <param name="db">Database.</param>
        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        
        /// <summary>
        /// Sletter brukerens konto.
        /// </summary>
        /// <returns>Sender til hjemmesiden eller viser feil.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // 1) Sjekk om brukeren har rapporter
            var hasReports = _db.Reports.Any(r => r.UserId == user.Id);
            if (hasReports)
            {
                TempData["ErrorMessage"] = "You cannot delete your account because you have existing reports in the system.";
                return RedirectToAction("Index");
            }

            // 2) Slett bruker (OrgUsers og Notifications har cascade i ApplicationContext)
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Could not delete your account. Please contact an administrator.";
                return RedirectToAction("Index");
            }

            // 3) Logg ut og send til forsiden
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Your account has been deleted.";
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Viser brukerens profil.
        /// </summary>
        /// <returns>Profilside.</returns>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Hvis noe er rart -> send til login
                return Challenge();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Finn organisasjonene (via join-tabellen OrganizationUsers)
            var orgs = _db.OrganizationUsers
                .Where(ou => ou.UserId == user.Id)
                .Select(ou => ou.Organization!.Name)   
                .ToList();

            var vm = new MyProfileViewModel
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Roles = roles.ToList(),
                Organizations = orgs
            };

            return View(vm);
            
            
        }
    }
}