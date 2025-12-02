using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models;

/// <summary>
/// Representerer en hindringsrapport som er sendt inn av en pilot eller entreprenør.
/// Lagres i Reports-tabellen i databasen. Inneholder informasjon om posisjon, hindertype, høyde og beskrivelse.
/// </summary>
public class Report
{
    /// <summary>
    /// Unik rapport-ID (primærnøkkel).
    /// Format: char(10) - for eksempel "REP2310141530"
    /// </summary>
    [Key]
    [Column(TypeName = "char(10)")]
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// Fremmednøkkel til AspNetUsers-tabellen (Identity).
    /// Knytter denne rapporten til brukeren som opprettet den.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Breddegrad for hindringen.
    /// Presisjon: decimal(11,9)
    /// </summary>
    [Column(TypeName = "decimal(11,9)")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Lengdegrad for hindringen.
    /// Presisjon: decimal(12,9)
    /// </summary>
    [Column(TypeName = "decimal(12,9)")]
    public decimal? Longitude { get; set; }
    
    /// <summary>
    /// Lagrer kart-geometrien (JSON) for linjer, polygoner og sirkler.
    /// Brukes når hindringen ikke er et enkelt punkt, men en linje eller et område.
    /// </summary>
    [Column(TypeName = "Text")]
    public string? Geometry { get; set; }

    /// <summary>
    /// Høyde i fot (valgfritt).
    /// Datatype: smallint
    /// Validering: valgfritt, men når angitt må være mellom 0 og 3000.
    /// </summary>
    [Column("HeightFeet", TypeName = "smallint")]
    [Range(0, 3000, ErrorMessage = "Height must be between 0 and 3000 feet.")]
    [Display(Name = "Height (feet)")]
    public short? HeightFeet { get; set; }


    /// <summary>
    /// Fremmednøkkel til ObstacleTypes-tabellen.
    /// 3-bokstavers kode (for eksempel "CRN", "MST", "TWR")
    /// </summary>
    [MaxLength(3)]
    public string? ObstacleId { get; set; }

    /// <summary>
    /// Detaljert beskrivelse av hindringen.
    /// Påkrevd felt ved innsending, minimum 10 tegn, maksimum 5000 tegn.
    /// </summary>
    [StringLength(5000, MinimumLength = 10)]
    [Column(TypeName = "text")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Valgfri kommentar fra registratoren ved godkjenning eller avvisning.
    /// Maksimalt 1000 tegn.
    /// </summary>
    [MaxLength(1000)]
    public string? RegistrarComment { get; set; }


    /// <summary>
    /// Dato og klokkeslett når rapporten ble opprettet.
    /// Settes automatisk i controller når rapporten opprettes.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Status på rapporten: "Draft", "Pending", "Approved" eller "Rejected".
    /// "Draft" = kladd, "Pending" = venter på gjennomgang, "Approved" = godkjent, "Rejected" = avvist.
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    // Navigasjonsegenskaper

    /// <summary>
    /// Navigasjonsegenskap: Brukeren som opprettet denne rapporten (fra AspNetUsers).
    /// Entity Framework bruker dette til å håndtere relasjonen mellom rapport og bruker.
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Navigasjonsegenskap: Hindertypen.
    /// Entity Framework bruker dette til å håndtere relasjonen mellom rapport og hindertype.
    /// </summary>
    [ForeignKey("ObstacleId")]
    public ObstacleTypeEntity? ObstacleType { get; set; }
    
    /// <summary>
    /// Dato og klokkeslett når rapporten sist ble oppdatert.
    /// Oppdateres automatisk når status eller andre felter endres.
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}