using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Representerer en luftfartsorganisasjon, for eksempel Norsk Luftambulanse, Luftforsvaret, Politiet.
    /// Brukes til å gruppere brukere og filtrere rapporter per organisasjon.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Primærnøkkel (identitet).
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Fullt navn på organisasjonen, for eksempel "Norsk Luftambulanse".
        /// Påkrevd felt, maksimalt 100 tegn.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kortkode for organisasjonen, for eksempel "NLA", "LFS", "PHT".
        /// Brukes for rask identifikasjon og filtrering.
        /// Påkrevd felt, maksimalt 10 tegn.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>
        /// Alle brukere som tilhører denne organisasjonen (via koblingstabell).
        /// Entity Framework bruker dette til å håndtere relasjonen mellom organisasjon og brukere.
        /// </summary>
        public ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
    }
}