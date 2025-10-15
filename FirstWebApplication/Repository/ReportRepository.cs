
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
    /// Repository for håndtering av Report-entiteter i databasen.
    /// Implementerer IReportRepository interface.
    /// Innkapsler all database-tilgangslogikk for rapporter.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Constructor som injiserer ApplicationContext via Dependency Injection.
        /// </summary>
        /// <param name="context">Database context for tilgang til Reports tabell</param>
        public ReportRepository(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle rapporter fra databasen asynkront.
        /// Bruker Entity Framework's Include() for å laste inn relaterte entiteter (eager loading).
        /// Dette unngår N+1 query-problemet ved å hente alt i én spørring.
        /// </summary>
        /// <returns>
        /// Liste med alle rapporter, hver med:
        /// - User: Brukeren som opprettet rapporten (med username, navn, osv.)
        /// - ObstacleType: Hindring-typen (med navn og ID)
        /// </returns>
        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .Include(r => r.User)           // Laster inn bruker-informasjon (JOIN med Users tabell)
                .Include(r => r.ObstacleType)   // Laster inn hindring-type (JOIN med ObstacleTypes tabell)
                .ToListAsync();                 // Asynkron konvertering til liste
        }

        /// <summary>
        /// Legger til en ny rapport i databasen asynkront.
        /// Setter automatisk følgende felter:
        /// - ReportId: Unik 10-tegns ID generert fra timestamp og GUID
        /// - DateTime: Nåværende tidspunkt
        /// - Status: "Pending" (venter på admin-godkjenning)
        /// </summary>
        /// <param name="report">
        /// Rapport-objektet fra controlleren.
        /// UserId, ObstacleId, Description, Latitude, Longitude må være satt.
        /// </param>
        /// <returns>Den opprettede rapporten med generert ReportId</returns>
        public async Task<Report> AddAsync(Report report)
        {
            // Generer en garantert unik ReportId (10 tegn)
            report.ReportId = await GenerateUniqueReportId();
            
            // Sett tidspunkt til nå (lokal tid)
            report.DateTime = DateTime.Now;
            
            // Sett default status til "Pending" (venter på godkjenning)
            report.Status = "Pending";

            // Legg til i DbContext (markerer som "Added" i change tracker)
            _context.Reports.Add(report);
            
            // Lagre endringer til database asynkront
            await _context.SaveChangesAsync();
            
            return report;
        }

        /// <summary>
        /// Oppdaterer en eksisterende rapport i databasen asynkront.
        /// Brukes hovedsakelig av admin for å endre status fra "Pending" til "Approved" eller "Rejected".
        /// Entity Framework tracker automatisk hvilke properties som er endret.
        /// </summary>
        /// <param name="report">Rapport-objektet med oppdaterte verdier (må ha gyldig ReportId)</param>
        /// <returns>Den oppdaterte rapporten</returns>
        public async Task<Report> UpdateAsync(Report report)
        {
            // Marker entiteten som "Modified" i change tracker
            _context.Reports.Update(report);
            
            // Lagre endringer til database asynkront
            await _context.SaveChangesAsync();
            
            return report;
        }

        /// <summary>
        /// Genererer en garantert unik ReportId.
        /// Format: "R" + 8 tegn timestamp + 4 tegn GUID (totalt 10 tegn, men kutter til 10).
        /// Eksempel: "R25101410" + "a3f2" = "R25101410a" (10 tegn).
        /// 
        /// Algoritme:
        /// 1. Lager timestamp (YYMMDDHHmm) for å sikre kronologisk sortering
        /// 2. Legger til 4 tilfeldige tegn fra GUID for å unngå kollisjoner
        /// 3. Sjekker database om ID allerede eksisterer
        /// 4. Gjentar til en unik ID er funnet
        /// </summary>
        /// <returns>Unik 10-tegns ReportId som starter med "R"</returns>
        private async Task<string> GenerateUniqueReportId()
        {
            string reportId;
            bool exists;

            do
            {
                // Timestamp: yyMMddHHmm (år, måned, dag, time, minutt)
                // Eksempel: 251014 1045 = 14. oktober 2025, kl 10:45
                var timestamp = DateTime.Now.ToString("yyMMddHHmm"); // 10 tegn
                
                // GUID: Globally Unique Identifier (128-bit tilfeldig tall)
                // ToString("N") fjerner bindestreker: "a3f2b5c7..."
                // Substring(0, 4) tar første 4 tegn: "a3f2"
                var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
                
                // Kombiner og kutt til 10 tegn:
                // "R" + timestamp (første 8) + guid (første 4) = "R25101410a3f2" -> kutt til 10
                reportId = $"R{timestamp.Substring(0, 8)}{guid}".Substring(0, 10);

                // Sjekk om denne ID-en allerede eksisterer i databasen
                exists = await _context.Reports.AnyAsync(r => r.ReportId == reportId);
            }
            while (exists); // Gjentar hvis ID allerede finnes (svært sjeldent)

            return reportId;
        }
    }
}