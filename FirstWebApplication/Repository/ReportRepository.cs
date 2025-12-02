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
    /// Repository-implementasjon for Report-entiteten.
    /// Bruker Entity Framework Core for dataaksess og implementerer IReportRepository-grensesnittet.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Oppretter en ny instans av ReportRepository med den angitte databasekonteksten.
        /// </summary>
        /// <param name="context">Databasekonteksten som skal brukes for dataaksess</param>
        public ReportRepository(ApplicationContext context)
        {
            _context = context;
        }

        // ===== HENTING AV DATA =====

        /// <summary>
        /// Henter alle rapporter med tilknyttet bruker- og hindertypedata.
        /// </summary>
        /// <returns>En liste over alle rapporter i systemet med relatert data</returns>
        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .ToListAsync();
        }

        /// <summary>
        /// Henter en enkelt rapport basert på ID med tilknyttet data.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal hentes</param>
        /// <returns>Rapporten hvis funnet, ellers null</returns>
        public async Task<Report?> GetByIdAsync(string id)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }

        /// <summary>
        /// Henter alle rapporter for en spesifikk bruker.
        /// </summary>
        /// <param name="userId">ID-en til brukeren hvis rapporter skal hentes</param>
        /// <returns>En liste over alle rapporter som tilhører brukeren, sortert med nyeste først</returns>
        public async Task<List<Report>> GetByUserIdAsync(string userId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.DateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Henter alle rapporter med en spesifikk status.
        /// </summary>
        /// <param name="status">Statusen som rapporter skal filtreres på (for eksempel "Pending", "Approved", "Rejected")</param>
        /// <returns>En liste over alle rapporter med den angitte statusen, sortert med nyeste først</returns>
        public async Task<List<Report>> GetByStatusAsync(string status)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.DateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Sjekker om en rapport eksisterer basert på ID.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal sjekkes</param>
        /// <returns>True hvis rapporten eksisterer, ellers false</returns>
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Reports.AnyAsync(r => r.ReportId == id);
        }

        // ===== ENDRING AV DATA =====

        /// <summary>
        /// Legger til en ny rapport i databasen. Genererer automatisk ReportId hvis den ikke er satt.
        /// </summary>
        /// <param name="report">Rapporten som skal legges til</param>
        /// <returns>Den lagrede rapporten med generert ID hvis nødvendig</returns>
        public async Task<Report> AddAsync(Report report)
        {
            // Generer unik ID hvis ikke satt
            if (string.IsNullOrWhiteSpace(report.ReportId))
                report.ReportId = await GenerateUniqueReportIdAsync();

            // Sett opprettelsestidspunkt hvis ikke satt
            if (report.DateTime == default)
                report.DateTime = DateTime.Now;

            // Standard status til Pending hvis ikke satt
            if (string.IsNullOrWhiteSpace(report.Status))
                report.Status = "Pending";

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Oppdaterer en eksisterende rapport i databasen.
        /// </summary>
        /// <param name="report">Rapporten som skal oppdateres</param>
        public async Task UpdateAsync(Report report)
        {
            report.LastUpdated = DateTime.Now;
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Sletter en rapport fra databasen basert på ID.
        /// </summary>
        /// <param name="id">ID-en til rapporten som skal slettes</param>
        public async Task DeleteAsync(string id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
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

        // ===== PRIVATE Hjelpemetoder =====

        /// <summary>
        /// Genererer en unik rapport-ID basert på tidsstempel og GUID.
        /// Format: R + 8 siffer fra tidsstempel + 4 tegn fra GUID = 10 tegn totalt.
        /// </summary>
        /// <returns>En unik rapport-ID som ikke allerede eksisterer i databasen</returns>
        private async Task<string> GenerateUniqueReportIdAsync()
        {
            string reportId;
            bool exists;

            do
            {
                var timestamp = DateTime.Now.ToString("yyMMddHHmm");
                var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
                reportId = $"R{timestamp.Substring(0, 8)}{guid}".Substring(0, 10);
                exists = await _context.Reports.AnyAsync(r => r.ReportId == reportId);
            }
            while (exists);

            return reportId;
        }
    }
}
