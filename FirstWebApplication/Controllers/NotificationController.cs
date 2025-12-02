using System.Threading.Tasks;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
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

        // GET: /Notification
        // Shows all notifications for the logged-in user (newest first).
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

        // GET: /Notification/Open/5
        // Mark notification as read and redirect to the report (if linked).
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

        // POST: /Notification/MarkAllRead
        // Mark all notifications as read for the current user.
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
