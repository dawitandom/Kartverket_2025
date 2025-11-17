using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Represents an aviation organization, e.g. NLA, Air Force, Police.
    /// Used to group users and filter reports per organization.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Primary key (identity).
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Full name, e.g. "Norsk Luftambulanse".
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short code, e.g. "NLA", "LFS", "PHT".
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>
        /// All users that belong to this organization (via join table).
        /// </summary>
        public ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
    }
}