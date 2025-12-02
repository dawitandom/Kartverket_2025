using System.Collections.Generic;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Data for å vise brukerens profil.
    /// </summary>
    public class MyProfileViewModel
    {
        /// <summary>
        /// Brukernavn.
        /// </summary>
        public string UserName { get; set; } = "";
        
        /// <summary>
        /// E-postadresse.
        /// </summary>
        public string Email { get; set; } = "";
        
        /// <summary>
        /// Liste over brukerens roller.
        /// </summary>
        public List<string> Roles { get; set; } = new();
        
        /// <summary>
        /// Liste over organisasjoner brukeren tilhører.
        /// </summary>
        public List<string> Organizations { get; set; } = new();
    }
}