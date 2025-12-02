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
    /// Repository-implementasjon for Notification-entiteten.
    /// Bruker Entity Framework Core for dataaksess og implementerer INotificationRepository-grensesnittet.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Oppretter en ny instans av NotificationRepository med den angitte databasekonteksten.
        /// </summary>
        /// <param name="context">Databasekonteksten som skal brukes for dataaksess</param>
        public NotificationRepository(ApplicationContext context)
        {
            _context = context;
        }

        // ===== HENTING AV DATA =====

        /// <summary>
        /// Henter alle varsler for en bruker, sortert med nyeste først.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som varslene tilhører</param>
        /// <returns>En liste over alle varsler for brukeren, sortert med nyeste først</returns>
        public async Task<List<Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Henter et varsel basert på ID, inkludert tilknyttet rapportdata.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal hentes</param>
        /// <returns>Varselet hvis funnet, ellers null</returns>
        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Report)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        /// <summary>
        /// Henter et varsel basert på ID, men bare hvis det tilhører den angitte brukeren.
        /// Dette sikrer at brukere bare kan se sine egne varsler.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal hentes</param>
        /// <param name="userId">ID-en til brukeren som skal eie varselet</param>
        /// <returns>Varselet hvis funnet og tilhører brukeren, ellers null</returns>
        public async Task<Notification?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Notifications
                .Include(n => n.Report)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        /// <summary>
        /// Henter antall uleste varsler for en bruker.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som varslene tilhører</param>
        /// <returns>Antall uleste varsler for brukeren</returns>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // ===== ENDRING AV DATA =====

        /// <summary>
        /// Oppretter et nytt varsel i databasen.
        /// </summary>
        /// <param name="notification">Varselet som skal opprettes</param>
        /// <returns>Det opprettede varselet med oppdatert ID</returns>
        public async Task<Notification> AddAsync(Notification notification)
        {
            if (notification.CreatedAt == default)
                notification.CreatedAt = DateTime.Now;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        /// <summary>
        /// Oppretter et varsel for en statusendring på en rapport.
        /// Dette er en hjelpemetode som forenkler opprettelsen av varsler knyttet til rapporter.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som skal motta varselet</param>
        /// <param name="reportId">ID-en til rapporten som varselet er knyttet til</param>
        /// <param name="title">Tittelen på varselet</param>
        /// <param name="message">Meldingen i varselet</param>
        /// <returns>Det opprettede varselet</returns>
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

        /// <summary>
        /// Markerer et varsel som lest.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal markeres som lest</param>
        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Markerer alle varsler for en bruker som lest.
        /// </summary>
        /// <param name="userId">ID-en til brukeren hvis varsler skal markeres som lest</param>
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

        /// <summary>
        /// Sletter et varsel fra databasen.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal slettes</param>
        public async Task DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        // ===== LAGRING AV ENDRINGER =====

        /// <summary>
        /// Lagrer alle ventende endringer til databasen.
        /// Brukes når man ønsker eksplisitt kontroll over når endringer skal lagres.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

