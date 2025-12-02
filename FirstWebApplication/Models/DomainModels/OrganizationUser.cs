using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Koblingstabell mellom ApplicationUser og Organization (mange-til-mange).
    /// En bruker kan tilhøre én eller flere organisasjoner, og en organisasjon kan ha mange brukere.
    /// Primærnøkkelen er en sammensatt nøkkel av OrganizationId og UserId.
    /// </summary>
    public class OrganizationUser
    {
        /// <summary>
        /// Fremmednøkkel til Organization.
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Navigasjonsegenskap: Organisasjonen som brukeren tilhører.
        /// </summary>
        public Organization Organization { get; set; } = null!;

        /// <summary>
        /// Fremmednøkkel til AspNetUsers (ApplicationUser).
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigasjonsegenskap: Brukeren som tilhører organisasjonen.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
    }
}