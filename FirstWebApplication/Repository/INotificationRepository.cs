using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository interface for Notification entity.
    /// Handles user notifications for report status changes.
    /// </summary>
    public interface INotificationRepository
    {
        // ===== READ OPERATIONS =====

        /// <summary>
        /// Gets all notifications for a user, ordered by most recent first.
        /// </summary>
        Task<List<Notification>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Gets a notification by ID (with related Report data).
        /// </summary>
        Task<Notification?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a notification by ID only if it belongs to the specified user.
        /// </summary>
        Task<Notification?> GetByIdForUserAsync(int id, string userId);

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);

        // ===== WRITE OPERATIONS =====

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        Task<Notification> AddAsync(Notification notification);

        /// <summary>
        /// Creates a notification for a report status change.
        /// </summary>
        Task<Notification> CreateForReportStatusChangeAsync(
            string userId, 
            string reportId, 
            string title, 
            string message);

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        Task MarkAsReadAsync(int id);

        /// <summary>
        /// Marks all notifications for a user as read.
        /// </summary>
        Task MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        Task DeleteAsync(int id);

        // ===== UNIT OF WORK =====

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync();
    }
}

