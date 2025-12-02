using System.Collections.Generic;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// ViewModel for å vise brukerens egen profilside.
    /// Brukes i ProfileController for å vise informasjon om den innloggede brukeren,
    /// inkludert brukernavn, e-post, roller og organisasjoner.
    /// </summary>
    public class MyProfileViewModel
    {
        /// <summary>
        /// Brukernavnet til den innloggede brukeren.
        /// </summary>
        public string UserName { get; set; } = "";
        
        /// <summary>
        /// E-postadressen til den innloggede brukeren.
        /// </summary>
        public string Email { get; set; } = "";
        
        /// <summary>
        /// Liste over alle roller som brukeren har i systemet.
        /// For eksempel "Pilot", "Entrepreneur", "DefaultUser", "Registrar", "Admin" eller "OrgAdmin".
        /// </summary>
        public List<string> Roles { get; set; } = new();
        
        /// <summary>
        /// Liste over navnene på organisasjonene som brukeren tilhører.
        /// Hvis brukeren ikke tilhører noen organisasjoner, vil listen være tom.
        /// </summary>
        public List<string> Organizations { get; set; } = new();
    }
}