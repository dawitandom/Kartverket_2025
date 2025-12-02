using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Entity Framework Core-basert implementasjon av IOrganizationRepository.
    /// Sentraliserer organisasjons- og medlemskapsspørringer slik at controllere
    /// unngår duplisering av LINQ-logikk.
    /// </summary>
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Oppretter en ny instans av OrganizationRepository med den angitte databasekonteksten.
        /// </summary>
        /// <param name="context">Databasekonteksten som skal brukes for dataaksess</param>
        public OrganizationRepository(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle organisasjoner sortert etter navn.
        /// </summary>
        /// <returns>En liste over alle organisasjoner i systemet</returns>
        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Finner en organisasjon basert på primærnøkkel.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen som skal hentes</param>
        /// <returns>Organisasjonen hvis funnet, ellers null</returns>
        public async Task<Organization?> GetByIdAsync(int organizationId)
        {
            return await _context.Organizations.FindAsync(organizationId);
        }

        /// <summary>
        /// Lagrer en nyopprettet organisasjon i databasen.
        /// </summary>
        /// <param name="organization">Organisasjonen som skal lagres</param>
        /// <returns>Den lagrede organisasjonen med oppdatert ID</returns>
        public async Task<Organization> AddAsync(Organization organization)
        {
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        /// <summary>
        /// Finner en organisasjon basert på kortkoden (case-sensitive).
        /// </summary>
        /// <param name="shortCode">Kortkoden til organisasjonen (for eksempel "NLA", "LFS")</param>
        /// <returns>Organisasjonen hvis funnet, ellers null</returns>
        public async Task<Organization?> GetByShortCodeAsync(string shortCode)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.ShortCode == shortCode);
        }

        /// <summary>
        /// Henter navnet på den første organisasjonen brukeren tilhører (hvis noen).
        /// </summary>
        /// <param name="userId">ID-en til brukeren</param>
        /// <returns>Navnet på organisasjonen hvis brukeren tilhører en, ellers null</returns>
        public async Task<string?> GetFirstOrganizationNameForUserAsync(string userId)
        {
            return await _context.OrganizationUsers
                .Where(ou => ou.UserId == userId)
                .Include(ou => ou.Organization)
                .Select(ou => ou.Organization!.Name)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finner hvilken organisasjon en organisasjonsadministrator tilhører.
        /// Prøver først å matche brukernavn med organisasjons shortcode, deretter OrganizationUsers-tabellen.
        /// </summary>
        /// <param name="userId">ID-en til organisasjonsadministratoren</param>
        /// <param name="userName">Brukernavnet til organisasjonsadministratoren (kan være null)</param>
        /// <returns>Organisasjons-ID-en hvis funnet, ellers null</returns>
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

        /// <summary>
        /// Henter alle OrganizationUser-rader (med User-navigasjon) for en organisasjon.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <returns>En liste over alle medlemmer i organisasjonen med brukerinformasjon</returns>
        public async Task<IReadOnlyList<OrganizationUser>> GetMembersWithUserAsync(int organizationId)
        {
            return await _context.OrganizationUsers
                .Where(ou => ou.OrganizationId == organizationId)
                .Include(ou => ou.User)
                .OrderBy(ou => ou.User!.UserName)
                .ToListAsync();
        }

        /// <summary>
        /// Returnerer en ordbok som mapper bruker-ID til liste over organisasjonskortkoder.
        /// Dette er nyttig for å vise organisasjoner for flere brukere uten å gjøre separate spørringer.
        /// </summary>
        /// <returns>En ordbok hvor nøkkelen er bruker-ID og verdien er en liste over organisasjonskortkoder</returns>
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

        /// <summary>
        /// Henter alle bruker-ID-er som tilhører en organisasjon identifisert ved kortkode.
        /// </summary>
        /// <param name="shortCode">Kortkoden til organisasjonen</param>
        /// <returns>En liste over alle bruker-ID-er som tilhører organisasjonen</returns>
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

        /// <summary>
        /// Sjekker om den angitte brukeren allerede tilhører organisasjonen.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren</param>
        /// <returns>True hvis brukeren tilhører organisasjonen, ellers false</returns>
        public async Task<bool> MemberExistsAsync(int organizationId, string userId)
        {
            return await _context.OrganizationUsers.AnyAsync(ou =>
                ou.OrganizationId == organizationId && ou.UserId == userId);
        }

        /// <summary>
        /// Legger til en bruker i organisasjonen. Gjør ingenting hvis brukeren allerede er medlem.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren som skal legges til</param>
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

        /// <summary>
        /// Fjerner en bruker fra organisasjonen hvis brukeren er medlem.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren som skal fjernes</param>
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

