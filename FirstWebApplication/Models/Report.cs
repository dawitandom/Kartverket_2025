using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Representerer en hindring-rapport (obstacle report) sendt inn av en pilot.
    /// Lagres i Reports tabellen i databasen.
    /// Inneholder informasjon om hindringens type, lokasjon, beskrivelse og status.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Unik ID for rapporten (Primary Key).
        /// Genereres automatisk i ReportRepository.AddAsync().
        /// Format: 10 tegn (f.eks. "R251014123").
        /// Datatype: char(10) - fast lengde.
        /// </summary>
        public string? ReportId { get; set; }
        
        /// <summary>
        /// Foreign Key til Users tabellen - ID på brukeren som opprettet rapporten.
        /// Settes automatisk i ReportController basert på innlogget bruker.
        /// Datatype: char(5) - fast lengde.
        /// Påkrevd felt.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Breddegrad (latitude) for hindringens posisjon.
        /// Gyldige verdier: -90 til 90 grader.
        /// Datatype: decimal(11,9) - 11 siffer totalt, 9 desimaler.
        /// Valgfritt felt.
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }
        
        /// <summary>
        /// Lengdegrad (longitude) for hindringens posisjon.
        /// Gyldige verdier: -180 til 180 grader.
        /// Datatype: decimal(12,9) - 12 siffer totalt, 9 desimaler.
        /// Valgfritt felt.
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }
        
        /// <summary>
        /// Høyde på hindringen i fot (feet).
        /// Gyldige verdier: 0 til 50000 fot.
        /// Datatype: smallint (16-bit integer).
        /// Valgfritt felt.
        /// </summary>
        [Range(0, 50000, ErrorMessage = "Altitude must be between 0 and 50000 feet")]
        public short? AltitudeFeet { get; set; }
        
        /// <summary>
        /// Foreign Key til ObstacleTypes tabellen - type hindring.
        /// 3-bokstavers kode (f.eks. "CRN", "MST", "TWR").
        /// Påkrevd felt, maks 3 tegn.
        /// </summary>
        [Required(ErrorMessage = "Obstacle type is required")]
        public string ObstacleId { get; set; } = null!;
        
        /// <summary>
        /// Detaljert beskrivelse av hindringen.
        /// Må være mellom 10 og 5000 tegn.
        /// Datatype: text - ubegrenset lengde i database.
        /// Påkrevd felt.
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 5000 characters")]
        public string Description { get; set; } = null!;
        
        /// <summary>
        /// Tidspunkt når rapporten ble opprettet (for intern bruk).
        /// Settes automatisk til UTC tid.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Tidspunkt når rapporten ble sendt inn (vises til brukere).
        /// Settes automatisk i ReportRepository.AddAsync() til lokal tid.
        /// Påkrevd felt.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Status på rapporten:
        /// - "Pending": Venter på godkjenning fra admin
        /// - "Approved": Godkjent av admin
        /// - "Rejected": Avslått av admin
        /// Settes til "Pending" ved opprettelse, endres av admin.
        /// Påkrevd felt, maks 20 tegn.
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Navigation property: Brukeren som opprettet rapporten.
        /// Entity Framework laster automatisk inn User når Report hentes fra database.
        /// Many-to-One: Mange rapporter kan tilhøre samme bruker.
        /// </summary>
        public User? User { get; set; }
        
        /// <summary>
        /// Navigation property: Typen hindring rapporten gjelder.
        /// Entity Framework laster automatisk inn ObstacleType når Report hentes fra database.
        /// Many-to-One: Mange rapporter kan ha samme hindring-type.
        /// </summary>
        public ObstacleTypeEntity? ObstacleType { get; set; }
    }
}