using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository-grensesnitt for Report-entiteten.
    /// Gir dataaksessoperasjoner som følger Repository Pattern.
    /// Abstraherer dataaksesslogikken og gjør det enklere å teste og vedlikeholde koden.
    /// </summary>
    public interface IReportRepository
    {        
        /// <summary>
        /// Henter alle rapporter med tilknyttet bruker- og hindertypedata.
        /// </summary>
        /// <returns>En liste over alle rapporter i systemet med relatert data</returns>
        Task<List<Report>> GetAllAsync();

        /// <summary>
        /// Henter en enkelt rapport basert på ID med tilknyttet data.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal hentes</param>
        /// <returns>Rapporten hvis funnet, ellers null</returns>
        Task<Report?> GetByIdAsync(string id);

        /// <summary>
        /// Henter alle rapporter for en spesifikk bruker.
        /// </summary>
        /// <param name="userId">ID-en til brukeren hvis rapporter skal hentes</param>
        /// <returns>En liste over alle rapporter som tilhører brukeren</returns>
        Task<List<Report>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Henter alle rapporter med en spesifikk status.
        /// </summary>
        /// <param name="status">Statusen som rapporter skal filtreres på (for eksempel "Pending", "Approved", "Rejected")</param>
        /// <returns>En liste over alle rapporter med den angitte statusen</returns>
        Task<List<Report>> GetByStatusAsync(string status);

        /// <summary>
        /// Sjekker om en rapport eksisterer basert på ID.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal sjekkes</param>
        /// <returns>True hvis rapporten eksisterer, ellers false</returns>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Legger til en ny rapport i databasen. Genererer automatisk ReportId hvis den ikke er satt.
        /// </summary>
        /// <param name="report">Rapporten som skal legges til</param>
        /// <returns>Den lagrede rapporten med generert ID hvis nødvendig</returns>
        Task<Report> AddAsync(Report report);

        /// <summary>
        /// Oppdaterer en eksisterende rapport i databasen.
        /// </summary>
        /// <param name="report">Rapporten som skal oppdateres</param>
        Task UpdateAsync(Report report);

        /// <summary>
        /// Sletter en rapport fra databasen basert på ID.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal slettes</param>
        Task DeleteAsync(string id);

        // ===== LAGRING AV ENDRINGER =====

        /// <summary>
        /// Lagrer alle ventende endringer til databasen.
        /// Brukes når man ønsker eksplisitt kontroll over når endringer skal lagres.
        /// </summary>
        Task SaveChangesAsync();
    }
}
