using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for håndtering av varsler til brukere.
    /// Kun tilgjengelig for brukere med rollene Pilot, Entrepreneur eller DefaultUser.
    /// Lar brukere se sine varsler, markere dem som lest, og åpne varsler som lenker til rapporter.
    /// </summary>
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            INotificationRepository notificationRepository,
            UserManager<ApplicationUser> userManager)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Viser alle varsler for den innloggede brukeren, sortert med nyeste først.
        /// Viser også antall uleste varsler i ViewBag for å kunne vise dette i navigasjonen.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _notificationRepository.GetByUserIdAsync(user.Id);
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(user.Id);
            
            ViewBag.UnreadCount = unreadCount;
            return View(notifications);
        }

        /// <summary>
        /// Åpner et varsel ved å markere det som lest og sender brukeren til den tilknyttede rapporten hvis varslet er knyttet til en rapport.
        /// Hvis varslet ikke finnes eller ikke tilhører brukeren, vises en feilmelding og brukeren sendes tilbake til varsellisten.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal åpnes</param>
        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notification = await _notificationRepository.GetByIdForUserAsync(id, user.Id);

            if (notification == null)
            {
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction(nameof(Index));
            }

            // Mark as read
            await _notificationRepository.MarkAsReadAsync(id);

            // If linked to a report, redirect to details page
            if (!string.IsNullOrEmpty(notification.ReportId))
            {
                return RedirectToAction("Details", "Report", new { id = notification.ReportId });
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Markerer alle varsler for den innloggede brukeren som lest.
        /// Dette er nyttig når brukeren ønsker å rydde opp i varsellisten sin.
        /// Viser en bekreftelsesmelding etter at alle varsler er markert som lest.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _notificationRepository.MarkAllAsReadAsync(user.Id);
            TempData["SuccessMessage"] = "All notifications marked as read.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}
