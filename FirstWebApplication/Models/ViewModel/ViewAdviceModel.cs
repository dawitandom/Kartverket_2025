namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// View model for Advice-funksjonen.
    /// Brukes til å overføre data mellom view og controller.
    /// Merk: Dette er en eldre funksjonalitet som ikke brukes i den nåværende versjonen
    /// av Obstacle Reporting System.
    /// </summary>
    public class ViewAdviceModel
    {
        /// <summary>
        /// Unik ID for advice-objektet.
        /// </summary>
        public int ViewAdviceId { get; set; }
        
        /// <summary>
        /// Tittel på advice.
        /// </summary>
        public string ViewTitle { get; set; }
        
        /// <summary>
        /// Beskrivelse av advice.
        /// </summary>
        public string ViewDescription { get; set; }
    }
}