namespace FirstWebApplication.Models
{
    /// <summary>
    /// Representerer en hindring-type i databasen (Crane, Mast, Tower, osv.).
    /// Lagres i ObstacleTypes tabellen.
    /// Brukes til dropdown-valg når pilot oppretter ny rapport.
    /// </summary>
    public class ObstacleTypeEntity
    {
        /// <summary>
        /// Unik 3-bokstavers kode for hindring-typen (primærnøkkel).
        /// Eksempler: "CRN" (Crane), "MST" (Mast), "TWR" (Tower).
        /// Påkrevd felt, maksimalt 3 tegn.
        /// </summary>
        public string ObstacleId { get; set; } = null!;
        
        /// <summary>
        /// Fullt navn på hindring-typen (for eksempel "Crane", "Mast", "PowerLine").
        /// Vises i nedtrekkslister og rapporter.
        /// Påkrevd felt, maksimalt 30 tegn.
        /// </summary>
        public string ObstacleName { get; set; } = null!;
        
        /// <summary>
        /// Sorteringsrekkefølge i nedtrekkslister.
        /// Lavere tall = høyere prioritet/vises først.
        /// Eksempel: Crane = 1, Mast = 2, Other = 9
        /// </summary>
        public int SortedOrder { get; set; }
        
        /// <summary>
        /// Navigasjonsegenskap: Liste over alle rapporter med denne hindring-typen.
        /// Entity Framework bruker dette til å håndtere relasjonen mellom ObstacleType og Report.
        /// En-til-mange: En hindring-type kan ha mange rapporter.
        /// </summary>
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}