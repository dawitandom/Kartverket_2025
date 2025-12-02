using System.Collections.Generic;
using System.Threading.Tasks;
using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    /// <summary>
    /// Repository-grensesnitt for å spørre og vedlikeholde organisasjoner
    /// og deres medlemsrelasjoner.
    /// Følger Repository Pattern for å abstrahere dataaksesslogikken.
    /// </summary>
    public interface IOrganizationRepository
    {
        /// <summary>
        /// Henter alle organisasjoner sortert etter navn.
        /// </summary>
        /// <returns>En liste over alle organisasjoner i systemet</returns>
        Task<List<Organization>> GetAllAsync();

        /// <summary>
        /// Finner en organisasjon basert på primærnøkkel.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen som skal hentes</param>
        /// <returns>Organisasjonen hvis funnet, ellers null</returns>
        Task<Organization?> GetByIdAsync(int organizationId);

        /// <summary>
        /// Lagrer en nyopprettet organisasjon i databasen.
        /// </summary>
        /// <param name="organization">Organisasjonen som skal lagres</param>
        /// <returns>Den lagrede organisasjonen med oppdatert ID</returns>
        Task<Organization> AddAsync(Organization organization);

        /// <summary>
        /// Finner hvilken organisasjon en organisasjonsadministrator tilhører.
        /// Prøver først å matche brukernavn med organisasjons shortcode, deretter OrganizationUsers-tabellen.
        /// </summary>
        /// <param name="userId">ID-en til organisasjonsadministratoren</param>
        /// <param name="userName">Brukernavnet til organisasjonsadministratoren (kan være null)</param>
        /// <returns>Organisasjons-ID-en hvis funnet, ellers null</returns>
        Task<int?> ResolveOrgIdForAdminAsync(string userId, string? userName);

        /// <summary>
        /// Finner en organisasjon basert på kortkoden (case-sensitive).
        /// </summary>
        /// <param name="shortCode">Kortkoden til organisasjonen (for eksempel "NLA", "LFS")</param>
        /// <returns>Organisasjonen hvis funnet, ellers null</returns>
        Task<Organization?> GetByShortCodeAsync(string shortCode);

        /// <summary>
        /// Henter navnet på den første organisasjonen brukeren tilhører (hvis noen).
        /// </summary>
        /// <param name="userId">ID-en til brukeren</param>
        /// <returns>Navnet på organisasjonen hvis brukeren tilhører en, ellers null</returns>
        Task<string?> GetFirstOrganizationNameForUserAsync(string userId);

        /// <summary>
        /// Henter alle OrganizationUser-rader (med User-navigasjon) for en organisasjon.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <returns>En liste over alle medlemmer i organisasjonen med brukerinformasjon</returns>
        Task<IReadOnlyList<OrganizationUser>> GetMembersWithUserAsync(int organizationId);

        /// <summary>
        /// Returnerer en ordbok som mapper bruker-ID til liste over organisasjonskortkoder.
        /// Dette er nyttig for å vise organisasjoner for flere brukere uten å gjøre separate spørringer.
        /// </summary>
        /// <returns>En ordbok hvor nøkkelen er bruker-ID og verdien er en liste over organisasjonskortkoder</returns>
        Task<Dictionary<string, List<string>>> GetUserOrganizationLookupAsync();

        /// <summary>
        /// Henter alle bruker-ID-er som tilhører en organisasjon identifisert ved kortkode.
        /// </summary>
        /// <param name="shortCode">Kortkoden til organisasjonen</param>
        /// <returns>En liste over alle bruker-ID-er som tilhører organisasjonen</returns>
        Task<List<string>> GetUserIdsForOrganizationShortCodeAsync(string shortCode);

        /// <summary>
        /// Sjekker om den angitte brukeren allerede tilhører organisasjonen.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren</param>
        /// <returns>True hvis brukeren tilhører organisasjonen, ellers false</returns>
        Task<bool> MemberExistsAsync(int organizationId, string userId);

        /// <summary>
        /// Legger til en bruker i organisasjonen. Gjør ingenting hvis brukeren allerede er medlem.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren som skal legges til</param>
        Task AddMemberAsync(int organizationId, string userId);

        /// <summary>
        /// Fjerner en bruker fra organisasjonen hvis brukeren er medlem.
        /// </summary>
        /// <param name="organizationId">ID-en til organisasjonen</param>
        /// <param name="userId">ID-en til brukeren som skal fjernes</param>
        Task RemoveMemberAsync(int organizationId, string userId);
    }
}

