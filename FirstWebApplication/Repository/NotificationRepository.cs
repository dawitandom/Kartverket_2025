using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository implementation for Notification entity.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationContext _context;

        public NotificationRepository(ApplicationContext context)
        {
            _context = context;
        }

        // ===== READ OPERATIONS =====

        public async Task<List<Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Report)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Notifications
                .Include(n => n.Report)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // ===== WRITE OPERATIONS =====

        public async Task<Notification> AddAsync(Notification notification)
        {
            if (notification.CreatedAt == default)
                notification.CreatedAt = DateTime.Now;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> CreateForReportStatusChangeAsync(
            string userId,
            string reportId,
            string title,
            string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReportId = reportId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        // ===== UNIT OF WORK =====

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

