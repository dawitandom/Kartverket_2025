using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models;

/// <summary>
/// Rapport om en hindring som er rapportert inn av en bruker.
/// Inneholder informasjon om lokasjon, høyde, type og beskrivelse av hindringen.
/// </summary>
public class Report
{
    /// <summary>
    /// Unik identifikator for rapporten.
    /// Format: 10 tegn (f.eks. "REP2310141530").
    /// </summary>
    [Key]
    [Column(TypeName = "char(10)")]
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// ID til brukeren som laget og sendte inn rapporten.
    /// Kobler rapporten til en bruker i systemet.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Breddegrad for hvor hindringen befinner seg.
    /// Brukes sammen med lengdegrad for å finne posisjonen på kartet.
    /// </summary>
    [Column(TypeName = "decimal(11,9)")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Lengdegrad for hvor hindringen befinner seg.
    /// Brukes sammen med breddegrad for å finne posisjonen på kartet.
    /// </summary>
    [Column(TypeName = "decimal(12,9)")]
    public decimal? Longitude { get; set; }
    
    /// <summary>
    /// Kart-geometri lagret som JSON.
    /// Brukes for å lagre linjer, polygoner og sirkler på kartet.
    /// </summary>
    [Column(TypeName = "Text")]
    public string? Geometry { get; set; }

    /// <summary>
    /// Høyde på hindringen målt i fot.
    /// Valgfri verdi, men må være mellom 0 og 3000 fot hvis den er satt.
    /// </summary>
    [Column("HeightFeet", TypeName = "smallint")]
    [Range(0, 3000, ErrorMessage = "Height must be between 0 and 3000 feet.")]
    [Display(Name = "Height (feet)")]
    public short? HeightFeet { get; set; }

    /// <summary>
    /// Type hindring gitt som en 3-bokstavskode.
    /// Eksempler: "CRN" for kran, "MST" for mast, "TWR" for tårn.
    /// </summary>
    [MaxLength(3)]
    public string? ObstacleId { get; set; }

    /// <summary>
    /// Detaljert beskrivelse av hindringen.
    /// Må være minst 10 tegn og maksimalt 5000 tegn lang.
    /// </summary>
    [StringLength(5000, MinimumLength = 10)]
    [Column(TypeName = "text")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Kommentar fra registerfører når rapporten blir godkjent eller avvist.
    /// Kan brukes til å gi tilbakemelding til brukeren som sendte inn rapporten.
    /// </summary>
    [MaxLength(1000)]
    public string? RegistrarComment { get; set; }

    /// <summary>
    /// Dato og tid når rapporten ble opprettet og sendt inn.
    /// Settes automatisk når rapporten lagres første gang.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Status på rapporten.
    /// Kan være "Pending" (venter på behandling), "Approved" (godkjent) eller "Rejected" (avvist).
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Brukeren som laget og sendte inn rapporten.
    /// Kobler til ApplicationUser-tabellen.
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Hindringstypen som rapporten handler om.
    /// Kobler til ObstacleTypeEntity-tabellen.
    /// </summary>
    [ForeignKey("ObstacleId")]
    public ObstacleTypeEntity? ObstacleType { get; set; }
    
    /// <summary>
    /// Dato og tid for når rapporten sist ble oppdatert.
    /// Brukes for å spore endringer i rapporten.
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}