namespace FirstWebApplication.Models
{
    /// <summary>
    /// Enum for hindring-typer (obstacles).
    /// Merk: Dette er en eldre implementering som ikke brukes aktivt.
    /// Systemet bruker nå ObstacleTypeEntity (database-basert) i stedet for enum.
    /// Beholdes for bakoverkompatibilitet.
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>
        /// Ukjent eller uspesifisert hindring.
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Mast (f.eks. radio/TV-mast).
        /// </summary>
        Mast = 1,
        
        /// <summary>
        /// Kran (byggekran eller havnekran).
        /// </summary>
        Crane = 2,
        
        /// <summary>
        /// Kraftlinje.
        /// </summary>
        PowerLine = 3,
        
        /// <summary>
        /// Tårn (f.eks. kirketårn, vanntårn).
        /// </summary>
        Tower = 4,
        
        /// <summary>
        /// Bygning (høyhus eller annen struktur).
        /// </summary>
        Building = 5,
        
        /// <summary>
        /// Annen type hindring.
        /// </summary>
        Other = 9
    }
}