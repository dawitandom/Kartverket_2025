using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Organisasjon som brukere kan tilhøre.
    /// Eksempler: Norsk Luftambulanse (NLA), Luftforsvaret (LFS), Kartverket (KRT).
    /// Brukes til å gruppere brukere og filtrere rapporter.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Unik identifikator for organisasjonen.
        /// Genereres automatisk av databasen.
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Fullt navn på organisasjonen.
        /// Eksempler: "Norsk Luftambulanse", "Luftforsvaret", "Kartverket".
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kortkode for organisasjonen.
        /// Brukes til å identifisere organisasjonen raskt.
        /// Eksempler: "NLA", "LFS", "KRT".
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>
        /// Alle brukere som tilhører denne organisasjonen.
        /// Kobles gjennom OrganizationUser-tabellen.
        /// </summary>
        public ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
    }
}