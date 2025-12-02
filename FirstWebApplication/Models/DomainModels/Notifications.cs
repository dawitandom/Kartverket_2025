using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Varsel til en bruker i systemet. Brukes for å informere brukere om viktige hendelser,
    /// for eksempel når en rapport er godkjent eller avvist. Varsler kan være knyttet til en spesifikk rapport.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Unik identifikator for varselet (primærnøkkel).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fremmednøkkel til brukeren som skal motta varselet (fra AspNetUsers).
        /// Påkrevd felt.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigasjonsegenskap: Brukeren som skal motta varselet.
        /// Entity Framework bruker dette til å håndtere relasjonen mellom varsel og bruker.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Valgfri fremmednøkkel til en rapport. Brukes når varselet er knyttet til en spesifikk rapport,
        /// for eksempel når en rapport er godkjent eller avvist. Lar brukeren gå direkte til rapporten fra varselet.
        /// </summary>
        public string? ReportId { get; set; }

        /// <summary>
        /// Navigasjonsegenskap: Rapporten som varselet er knyttet til (hvis angitt).
        /// Entity Framework bruker dette til å håndtere relasjonen mellom varsel og rapport.
        /// </summary>
        public Report? Report { get; set; }

        /// <summary>
        /// Tittel på varselet, for eksempel "Rapport godkjent" eller "Rapport avvist".
        /// Påkrevd felt, maksimalt 100 tegn.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Meldingen i varselet som beskriver hva som har skjedd.
        /// Påkrevd felt, maksimalt 500 tegn.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Angir om varselet er lest eller ikke.
        /// False = ulest, True = lest.
        /// Standardverdi er false (ulest).
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Dato og klokkeslett når varselet ble opprettet.
        /// Settes automatisk til nåværende tidspunkt når varselet opprettes.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}