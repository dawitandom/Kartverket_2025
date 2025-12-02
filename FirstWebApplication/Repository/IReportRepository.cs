using FirstWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository interface for Report entity.
    /// Provides data access operations following the Repository Pattern.
    /// </summary>
    public interface IReportRepository
    {
        // ===== READ OPERATIONS =====
        
        /// <summary>
        /// Gets all reports with related User and ObstacleType data.
        /// </summary>
        Task<List<Report>> GetAllAsync();

        /// <summary>
        /// Gets a single report by ID with related data.
        /// </summary>
        Task<Report?> GetByIdAsync(string id);

        /// <summary>
        /// Gets all reports for a specific user.
        /// </summary>
        Task<List<Report>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Gets all reports with a specific status.
        /// </summary>
        Task<List<Report>> GetByStatusAsync(string status);

        /// <summary>
        /// Checks if a report exists by ID.
        /// </summary>
        Task<bool> ExistsAsync(string id);

        // ===== WRITE OPERATIONS =====

        /// <summary>
        /// Adds a new report. Auto-generates ReportId if not set.
        /// </summary>
        Task<Report> AddAsync(Report report);

        /// <summary>
        /// Updates an existing report.
        /// </summary>
        Task UpdateAsync(Report report);

        /// <summary>
        /// Deletes a report by ID.
        /// </summary>
        Task DeleteAsync(string id);

        // ===== UNIT OF WORK =====

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync();
    }
}
