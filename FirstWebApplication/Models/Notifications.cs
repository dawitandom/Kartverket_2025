using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Varsel som sendes til en bruker i systemet.
    /// Brukes til å informere brukere om endringer i deres rapporter, f.eks. når en rapport blir godkjent eller avvist.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Unik identifikator for varselet.
        /// Genereres automatisk av databasen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID til brukeren som skal motta varselet.
        /// Kobler til ApplicationUser (AspNetUsers) tabellen.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Brukeren som skal motta varselet.
        /// Navigation property til ApplicationUser.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// ID til rapporten som varselet handler om.
        /// Valgfri - brukes hvis varselet er knyttet til en spesifikk rapport.
        /// Hvis satt, kan brukeren klikke på varselet for å gå direkte til rapporten.
        /// </summary>
        public string? ReportId { get; set; }
        
        /// <summary>
        /// Rapporten som varselet handler om.
        /// Navigation property til Report.
        /// </summary>
        public Report? Report { get; set; }

        /// <summary>
        /// Kort tittel på varselet.
        /// Eksempler: "Rapport godkjent", "Rapport avvist".
        /// Maksimalt 100 tegn.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detaljert melding i varselet.
        /// Forklarer hva som har skjedd, f.eks. "Din rapport REP123456789 ble godkjent."
        /// Maksimalt 500 tegn.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Angir om varselet er lest av brukeren.
        /// false betyr at varselet er ulest, true betyr at det er lest.
        /// Brukes til å vise antall uleste varsler til brukeren.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Dato og tid når varselet ble opprettet og sendt.
        /// Settes automatisk når varselet lagres.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}