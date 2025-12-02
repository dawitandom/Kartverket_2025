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
    /// Controller for varsler.
    /// </summary>
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public class NotificationController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Lager en ny NotificationController.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="userManager">Håndterer brukere.</param>
        public NotificationController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>
        /// Viser alle varsler for brukeren.
        /// </summary>
        /// <returns>Liste over varsler.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _db.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var unreadCount = notifications.Count(n => !n.IsRead);
            ViewBag.UnreadCount = unreadCount;

            return View(notifications);
        }

        /// <summary>
        /// Åpner et varsel og markerer det som lest.
        /// </summary>
        /// <param name="id">Varsel-ID.</param>
        /// <returns>Sender til rapport eller varselliste.</returns>
        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notification = await _db.Notifications
                .Include(n => n.Report)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification == null)
            {
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }

            // Hvis varselet er knyttet til en rapport – gå til detaljsiden
            if (!string.IsNullOrEmpty(notification.ReportId))
            {
                return RedirectToAction("Details", "Report", new { id = notification.ReportId });
            }

            // Ellers bare tilbake til lista
            return RedirectToAction(nameof(Index));
        }
    }
}
