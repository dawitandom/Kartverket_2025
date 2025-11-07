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
    /// Repository for handling Report entities in database.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;

        public ReportRepository(ApplicationContext context)
        {
            _context = context;
        }

        // EXISTING ASYNC METHODS

        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .ToListAsync();
        }

        public async Task<Report> AddAsync(Report report)
        {
            // Generér ID hvis den ikke er satt (controlleren skal ikke sette den)
            if (string.IsNullOrWhiteSpace(report.ReportId))
                report.ReportId = await GenerateUniqueReportId();

            // Sett opprettelsestid hvis mangler
            if (report.DateTime == default)
                report.DateTime = DateTime.Now; // evt. DateTime.UtcNow

            // Behold status fra controller (Draft/Pending). Fallback til Pending.
            if (string.IsNullOrWhiteSpace(report.Status))
                report.Status = "Pending";

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }

        // NEW SYNC METHODS FOR COMPATIBILITY

        public IEnumerable<Report> GetAllReports()
        {
            return _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .ToList();
        }

        public Report? GetReportById(string id)
        {
            return _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .FirstOrDefault(r => r.ReportId == id);
        }

        public void AddReport(Report report)
        {
            if (string.IsNullOrWhiteSpace(report.ReportId))
                report.ReportId = GenerateUniqueReportId().GetAwaiter().GetResult();

            if (report.DateTime == default)
                report.DateTime = DateTime.Now;

            if (string.IsNullOrWhiteSpace(report.Status))
                report.Status = "Pending";

            _context.Reports.Add(report);
        }

        public void UpdateReport(Report report)
        {
            _context.Reports.Update(report);
        }

        public void DeleteReport(string id)
        {
            var report = _context.Reports.Find(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // HELPER METHOD

        private async Task<string> GenerateUniqueReportId()
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