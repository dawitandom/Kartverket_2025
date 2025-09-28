using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class Report
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Altitude (feet) is required.")]
        [Range(1, 100000, ErrorMessage = "Altitude must be between {1} and {2} feet.")]
        public int? AltitudeFeet { get; set; }

        [Required(ErrorMessage = "Obstacle type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid obstacle type.")]
        public ObstacleType Type { get; set; } = ObstacleType.Unknown;
    }
}