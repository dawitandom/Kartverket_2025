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
    /// Repository implementation for Report entity.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;

        public ReportRepository(ApplicationContext context)
        {
            _context = context;
        }

        // ===== READ OPERATIONS =====

        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .ToListAsync();
        }

        public async Task<Report?> GetByIdAsync(string id)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }

        public async Task<List<Report>> GetByUserIdAsync(string userId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.DateTime)
                .ToListAsync();
        }

        public async Task<List<Report>> GetByStatusAsync(string status)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ObstacleType)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.DateTime)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Reports.AnyAsync(r => r.ReportId == id);
        }

        // ===== WRITE OPERATIONS =====

        public async Task<Report> AddAsync(Report report)
        {
            // Generate unique ID if not set
            if (string.IsNullOrWhiteSpace(report.ReportId))
                report.ReportId = await GenerateUniqueReportIdAsync();

            // Set creation timestamp if not set
            if (report.DateTime == default)
                report.DateTime = DateTime.Now;

            // Default status to Pending if not set
            if (string.IsNullOrWhiteSpace(report.Status))
                report.Status = "Pending";

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task UpdateAsync(Report report)
        {
            report.LastUpdated = DateTime.Now;
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        // ===== UNIT OF WORK =====

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ===== PRIVATE HELPERS =====

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
