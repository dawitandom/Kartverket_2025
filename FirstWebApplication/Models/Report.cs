using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models;

/// <summary>
/// Represents an obstacle report submitted by a Pilot or Entrepreneur.
/// Stored in the Reports table in the database.
/// </summary>
public class Report
{
    /// <summary>
    /// Unique report ID (Primary Key).
    /// Format: char(10) - e.g., "REP2310141530"
    /// </summary>
    [Key]
    [Column(TypeName = "char(10)")]
    public string ReportId { get; set; } = string.Empty; // Changed from null! to string.Empty

    /// <summary>
    /// Foreign Key to AspNetUsers table (Identity).
    /// Links this report to the user who created it.
    /// </summary>
    public string UserId { get; set; } = string.Empty; // Removed [Required] and changed from null! to string.Empty

    /// <summary>
    /// Latitude coordinate of the obstacle.
    /// Precision: decimal(11,9)
    /// </summary>
    [Column(TypeName = "decimal(11,9)")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the obstacle.
    /// Precision: decimal(12,9)
    /// </summary>
    [Column(TypeName = "decimal(12,9)")]
    public decimal? Longitude { get; set; }
    
    // Lagrer kart-geometrien (JSON) for linjer, polygoner og sirkler.
    [Column(TypeName = "Text")]
    public string? Geometry { get; set; }

    /// <summary>
    /// Height in feet (optional).
    /// Data type: smallint
    /// Validation: optional, but when provided must be between 0 and 20,000.
    /// </summary>
    [Column("HeightFeet", TypeName = "smallint")]
    [Range(0, 20000, ErrorMessage = "Height must be between 0 and 20,000 feet.")]
    [Display(Name = "Height (feet)")]
    public short? HeightFeet { get; set; }


    /// <summary>
    /// Foreign Key to ObstacleTypes table.
    /// 3-character code (e.g., "CRN", "MST", "TWR")
    /// </summary>
    [MaxLength(3)]
    public string? ObstacleId { get; set; }

    /// <summary>
    /// Detailed description of the obstacle.
    /// Required field, minimum 10 characters, maximum 5000.
    /// </summary>
    [StringLength(5000, MinimumLength = 10)]
    [Column(TypeName = "text")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional comment from the registrar when approving/rejecting.
    /// </summary>
    [MaxLength(1000)]
    public string? RegistrarComment { get; set; }


    /// <summary>
    /// Date and time when the report was created.
    /// </summary>
    public DateTime DateTime { get; set; } // Removed [Required] - will be set in controller

    /// <summary>
    /// Status of the report: "Pending", "Approved", or "Rejected".
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Removed [Required] - has default value

    // Navigation properties

    /// <summary>
    /// Navigation property: The user who created this report (from AspNetUsers).
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Navigation property: The obstacle type.
    /// </summary>
    [ForeignKey("ObstacleId")]
    public ObstacleTypeEntity? ObstacleType { get; set; }
    
    public DateTime? LastUpdated { get; set; }
}