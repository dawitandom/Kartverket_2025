using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for the home page.
    /// Shows role-specific dashboard with quick actions.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays role-specific home page:
        /// - Pilot/Entrepreneur: Quick actions for creating and viewing reports
        /// - Registrar: Quick actions for pending and reviewed reports
        /// - Admin: Quick actions for user management
        /// </summary>
        public IActionResult Index()
        {
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