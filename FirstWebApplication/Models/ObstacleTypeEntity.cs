namespace FirstWebApplication.Models
{
    /// <summary>
    /// Type hindring som kan rapporteres inn.
    /// Brukes i dropdown-lister når brukere oppretter nye rapporter.
    /// </summary>
    public class ObstacleTypeEntity
    {
        /// <summary>
        /// Unik 3-bokstavskode som identifiserer hindringstypen.
        /// Dette er primærnøkkelen i databasen.
        /// </summary>
        public string ObstacleId { get; set; } = null!;
        
        /// <summary>
        /// Fullt navn på hindringstypen som vises til brukerne.
        /// Dette er det som vises i dropdown-lister og rapporter.
        /// </summary>
        public string ObstacleName { get; set; } = null!;
        
        /// <summary>
        /// Tall som bestemmer rekkefølgen hindringstypen vises i dropdown-lister.
        /// Lavere tall betyr at typen vises først i listen.
        /// Eksempel: Crane = 1 (vises først), Mast = 2, Other = 9 (vises sist).
        /// </summary>
        public int SortedOrder { get; set; }
        
        /// <summary>
        /// Alle rapporter som har denne hindringstypen.
        /// En hindringstype kan ha mange rapporter.
        /// </summary>
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}