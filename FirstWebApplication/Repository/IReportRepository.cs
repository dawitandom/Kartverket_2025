using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Interface for Report Repository.
    /// Defines contract for database operations on reports.
    /// Uses Repository Pattern to separate data access logic from business logic.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Gets all reports from database (async).
        /// Includes related data (User and ObstacleType).
        /// </summary>
        Task<List<Report>> GetAllAsync();
        
        /// <summary>
        /// Adds a new report to database (async).
        /// Auto-generates ReportId, sets DateTime and Status to "Pending".
        /// </summary>
        Task<Report> AddAsync(Report report);
        
        /// <summary>
        /// Updates an existing report (async).
        /// Used primarily by admin to change status.
        /// </summary>
        Task<Report> UpdateAsync(Report report);

        // NEW METHODS FOR COMPATIBILITY
        /// <summary>
        /// Gets all reports (sync wrapper).
        /// </summary>
        IEnumerable<Report> GetAllReports();

        /// <summary>
        /// Gets a single report by ID.
        /// </summary>
        Report? GetReportById(string id);

        /// <summary>
        /// Adds a report (sync).
        /// </summary>
        void AddReport(Report report);

        /// <summary>
        /// Updates a report (sync).
        /// </summary>
        void UpdateReport(Report report);

        /// <summary>
        /// Deletes a report by ID.
        /// </summary>
        void DeleteReport(string id);

        /// <summary>
        /// Saves changes to database.
        /// </summary>
        Task SaveChangesAsync();
    }
}