using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository-grensesnitt for Notification-entiteten.
    /// Håndterer brukervarsler for statusendringer på rapporter.
    /// Følger Repository Pattern for å abstrahere dataaksesslogikken.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Henter alle varsler for en bruker, sortert med nyeste først.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som varslene tilhører</param>
        /// <returns>En liste over alle varsler for brukeren, sortert med nyeste først</returns>
        Task<List<Notification>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Henter et varsel basert på ID, inkludert tilknyttet rapportdata.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal hentes</param>
        /// <returns>Varselet hvis funnet, ellers null</returns>
        Task<Notification?> GetByIdAsync(int id);

        /// <summary>
        /// Henter et varsel basert på ID, men bare hvis det tilhører den angitte brukeren.
        /// Dette sikrer at brukere bare kan se sine egne varsler.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal hentes</param>
        /// <param name="userId">ID-en til brukeren som skal eie varselet</param>
        /// <returns>Varselet hvis funnet og tilhører brukeren, ellers null</returns>
        Task<Notification?> GetByIdForUserAsync(int id, string userId);

        /// <summary>
        /// Henter antall uleste varsler for en bruker.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som varslene tilhører</param>
        /// <returns>Antall uleste varsler for brukeren</returns>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Oppretter et nytt varsel i databasen.
        /// </summary>
        /// <param name="notification">Varselet som skal opprettes</param>
        /// <returns>Det opprettede varselet med oppdatert ID</returns>
        Task<Notification> AddAsync(Notification notification);

        /// <summary>
        /// Oppretter et varsel for en statusendring på en rapport.
        /// Dette er en hjelpemetode som forenkler opprettelsen av varsler knyttet til rapporter.
        /// </summary>
        /// <param name="userId">ID-en til brukeren som skal motta varselet</param>
        /// <param name="reportId">ID-en til rapporten som varselet er knyttet til</param>
        /// <param name="title">Tittelen på varselet</param>
        /// <param name="message">Meldingen i varselet</param>
        /// <returns>Det opprettede varselet</returns>
        Task<Notification> CreateForReportStatusChangeAsync(
            string userId, 
            string reportId, 
            string title, 
            string message);

        /// <summary>
        /// Markerer et varsel som lest.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal markeres som lest</param>
        Task MarkAsReadAsync(int id);

        /// <summary>
        /// Markerer alle varsler for en bruker som lest.
        /// </summary>
        /// <param name="userId">ID-en til brukeren hvis varsler skal markeres som lest</param>
        Task MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Sletter et varsel fra databasen.
        /// </summary>
        /// <param name="id">ID-en til varselet som skal slettes</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Lagrer alle ventende endringer til databasen.
        /// Brukes når man ønsker eksplisitt kontroll over når endringer skal lagres.
        /// </summary>
        Task SaveChangesAsync();
    }
}

