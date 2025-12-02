using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Kobling mellom bruker og organisasjon.
    /// En bruker kan tilhøre flere organisasjoner, og en organisasjon kan ha mange brukere.
    /// Dette er en mange-til-mange-relasjon.
    /// </summary>
    public class OrganizationUser
    {
        /// <summary>
        /// ID til organisasjonen som brukeren tilhører.
        /// Kobler til Organization-tabellen.
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Organisasjonen som brukeren tilhører.
        /// Navigation property til Organization.
        /// </summary>
        public Organization Organization { get; set; } = null!;

        /// <summary>
        /// ID til brukeren som tilhører organisasjonen.
        /// Kobler til ApplicationUser (AspNetUsers) tabellen.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Brukeren som tilhører organisasjonen.
        /// Navigation property til ApplicationUser.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
    }
}