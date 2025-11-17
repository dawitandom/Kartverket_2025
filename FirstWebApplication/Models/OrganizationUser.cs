using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Join table between ApplicationUser and Organization (many-to-many).
    /// One user can belong to one or more organizations, and one organization can have many users.
    /// </summary>
    public class OrganizationUser
    {
        /// <summary>
        /// FK to Organization.
        /// </summary>
        public int OrganizationId { get; set; }

        public Organization Organization { get; set; } = null!;

        /// <summary>
        /// FK to AspNetUsers (ApplicationUser).
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}