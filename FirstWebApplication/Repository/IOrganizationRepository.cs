using System.Collections.Generic;
using System.Threading.Tasks;
using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository contract for querying and maintaining organizations
    /// and their member relationships.
    /// </summary>
    public interface IOrganizationRepository
    {
        /// <summary>
        /// Returns all organizations ordered by display name.
        /// </summary>
        Task<List<Organization>> GetAllAsync();

        /// <summary>
        /// Finds a single organization by primary key.
        /// </summary>
        Task<Organization?> GetByIdAsync(int organizationId);

        /// <summary>
        /// Persists a newly created organization.
        /// </summary>
        Task<Organization> AddAsync(Organization organization);

        /// <summary>
        /// Resolves which organization an OrgAdmin belongs to.
        /// Tries username-as-shortcode first, then OrganizationUsers.
        /// </summary>
        Task<int?> ResolveOrgIdForAdminAsync(string userId, string? userName);

        /// <summary>
        /// Finds an organization by its ShortCode (case sensitive).
        /// </summary>
        Task<Organization?> GetByShortCodeAsync(string shortCode);

        /// <summary>
        /// Returns the first organization name the user belongs to (if any).
        /// </summary>
        Task<string?> GetFirstOrganizationNameForUserAsync(string userId);

        /// <summary>
        /// Loads all OrganizationUser rows (with User navigation) for an organization.
        /// </summary>
        Task<IReadOnlyList<OrganizationUser>> GetMembersWithUserAsync(int organizationId);

        /// <summary>
        /// Returns dictionary userId -> list of organization short codes.
        /// </summary>
        Task<Dictionary<string, List<string>>> GetUserOrganizationLookupAsync();

        /// <summary>
        /// Returns all user IDs belonging to an organization identified by short code.
        /// </summary>
        Task<List<string>> GetUserIdsForOrganizationShortCodeAsync(string shortCode);

        /// <summary>
        /// Checks whether the provided user already belongs to the organization.
        /// </summary>
        Task<bool> MemberExistsAsync(int organizationId, string userId);

        /// <summary>
        /// Adds a user to the organization (no-op if already present).
        /// </summary>
        Task AddMemberAsync(int organizationId, string userId);

        /// <summary>
        /// Removes a user from the organization if present.
        /// </summary>
        Task RemoveMemberAsync(int organizationId, string userId);
    }
}

