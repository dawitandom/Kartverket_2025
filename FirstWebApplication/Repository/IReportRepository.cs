using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Interface for Report Repository.
    /// Definerer kontrakten for database-operasjoner på rapporter.
    /// Bruker Repository Pattern for å separere data-tilgangslogikk fra forretningslogikk.
    /// Gjør koden mer testbar og vedlikeholdbar (kan enkelt mocke for unit testing).
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Henter alle rapporter fra databasen.
        /// Inkluderer relaterte data (User og ObstacleType) via Entity Framework's Include().
        /// </summary>
        /// <returns>Liste med alle rapporter, inkludert bruker- og hindring-informasjon</returns>
        Task<List<Report>> GetAllAsync();
        
        /// <summary>
        /// Legger til en ny rapport i databasen.
        /// Genererer automatisk unik ReportId, setter DateTime og Status til "Pending".
        /// </summary>
        /// <param name="report">Rapport-objektet som skal lagres (uten ReportId)</param>
        /// <returns>Den opprettede rapporten med generert ReportId</returns>
        Task<Report> AddAsync(Report report);
        
        /// <summary>
        /// Oppdaterer en eksisterende rapport i databasen.
        /// Brukes primært av admin for å endre status (Approve/Reject).
        /// </summary>
        /// <param name="report">Rapport-objektet med oppdaterte verdier</param>
        /// <returns>Den oppdaterte rapporten</returns>
        Task<Report> UpdateAsync(Report report);
    }
}