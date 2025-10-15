
namespace FirstWebApplication.Models
{
    /// <summary>
    /// Representerer en bruker i systemet (pilot eller admin).
    /// Lagres i Users tabellen i databasen.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unik bruker-ID (Primary Key).
        /// Format: 5 tegn (f.eks. "USR01" for pilot, "ADM01" for admin).
        /// Datatype: char(5) - fast lengde.
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Brukernavn for innlogging.
        /// Må være unikt i systemet.
        /// Påkrevd felt, maks 30 tegn.
        /// </summary>
        public string Username { get; set; } = null!;
        
        /// <summary>
        /// Brukerens fornavn.
        /// Valgfritt felt, maks 30 tegn.
        /// </summary>
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Brukerens etternavn.
        /// Valgfritt felt, maks 30 tegn.
        /// </summary>
        public string? LastName { get; set; }
        
        /// <summary>
        /// E-postadresse til brukeren.
        /// Påkrevd felt, maks 200 tegn.
        /// </summary>
        public string Mail { get; set; } = null!;
        
        /// <summary>
        /// Passord for innlogging.
        /// OBS: Lagret i plain text kun for testing/utvikling!
        /// I produksjon bør dette haskes (f.eks. med BCrypt).
        /// Datatype: char(60) - fast lengde.
        /// Påkrevd felt.
        /// </summary>
        public string Password { get; set; } = null!;
        
        /// <summary>
        /// Foreign Key til UserRoles tabellen.
        /// Definerer brukerens rolle: 1 = Admin, 2 = User/Pilot.
        /// Datatype: smallint (16-bit integer).
        /// </summary>
        public short UserRoleId { get; set; }
        
        /// <summary>
        /// Navigation property: Rollen brukeren tilhører.
        /// Entity Framework laster automatisk inn UserRole når User hentes fra database.
        /// Many-to-One: Mange brukere kan ha samme rolle.
        /// </summary>
        public UserRole? UserRole { get; set; }
        
        /// <summary>
        /// Navigation property: Liste over alle rapporter opprettet av denne brukeren.
        /// Entity Framework bruker dette til å håndtere relasjonen mellom User og Report.
        /// One-to-Many: En bruker kan ha mange rapporter.
        /// </summary>
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}