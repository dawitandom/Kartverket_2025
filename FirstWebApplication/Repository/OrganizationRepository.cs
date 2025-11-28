using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// EF Core backed implementation for <see cref="IOrganizationRepository"/>.
    /// Centralizes organization + membership queries so controllers
    /// avoid duplicating LINQ logic.
    /// </summary>
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationContext _context;

        public OrganizationRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<Organization?> GetByIdAsync(int organizationId)
        {
            return await _context.Organizations.FindAsync(organizationId);
        }

        public async Task<Organization> AddAsync(Organization organization)
        {
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization?> GetByShortCodeAsync(string shortCode)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.ShortCode == shortCode);
        }

        public async Task<string?> GetFirstOrganizationNameForUserAsync(string userId)
        {
            return await _context.OrganizationUsers
                .Where(ou => ou.UserId == userId)
                .Include(ou => ou.Organization)
                .Select(ou => ou.Organization!.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> ResolveOrgIdForAdminAsync(string userId, string? userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var orgByShortCode = await _context.Organizations
                    .Where(o => o.ShortCode == userName)
                    .Select(o => (int?)o.OrganizationId)
                    .FirstOrDefaultAsync();

                if (orgByShortCode != null)
                    return orgByShortCode;
            }

            return await _context.OrganizationUsers
                .Where(ou => ou.UserId == userId)
                .Select(ou => (int?)ou.OrganizationId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<OrganizationUser>> GetMembersWithUserAsync(int organizationId)
        {
            return await _context.OrganizationUsers
                .Where(ou => ou.OrganizationId == organizationId)
                .Include(ou => ou.User)
                .OrderBy(ou => ou.User!.UserName)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetUserOrganizationLookupAsync()
        {
            return await _context.OrganizationUsers
                .Join(_context.Organizations,
                    ou => ou.OrganizationId,
                    o => o.OrganizationId,
                    (ou, o) => new { ou.UserId, OrganizationShortCode = o.ShortCode })
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => x.OrganizationShortCode).ToList());
        }

        public async Task<List<string>> GetUserIdsForOrganizationShortCodeAsync(string shortCode)
        {
            return await _context.OrganizationUsers
                .Join(_context.Organizations,
                    ou => ou.OrganizationId,
                    o => o.OrganizationId,
                    (ou, o) => new { ou.UserId, OrganizationShortCode = o.ShortCode })
                .Where(x => x.OrganizationShortCode == shortCode)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> MemberExistsAsync(int organizationId, string userId)
        {
            return await _context.OrganizationUsers.AnyAsync(ou =>
                ou.OrganizationId == organizationId && ou.UserId == userId);
        }

        public async Task AddMemberAsync(int organizationId, string userId)
        {
            var exists = await MemberExistsAsync(organizationId, userId);
            if (exists)
                return;

            _context.OrganizationUsers.Add(new OrganizationUser
            {
                OrganizationId = organizationId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int organizationId, string userId)
        {
            var link = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId);

            if (link == null)
                return;

            _context.OrganizationUsers.Remove(link);
            await _context.SaveChangesAsync();
        }
    }
}

