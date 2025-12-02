using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Notification til en bruker (f.eks. "Report approved/rejected").
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Valgfri kobling til en rapport (brukes for å sende bruker til riktig rapport).
        /// </summary>
        public string? ReportId { get; set; }
        public Report? Report { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// False = ulest, True = lest.
        /// </summary>
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}