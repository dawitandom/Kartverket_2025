using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// ViewModel for å vise og administrere brukere i systemet.
/// Brukes i AdminController for å vise en liste over alle brukere med deres roller og organisasjoner.
/// Lar administratorer se oversikt over alle brukere og deres tilknytninger.
/// </summary>
public class UserManagementViewModel
{
    /// <summary>
    /// Unik ID for brukeren i systemet (fra AspNetUsers-tabellen).
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukernavnet til brukeren.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// E-postadressen til brukeren.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Fornavnet til brukeren.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Etternavnet til brukeren.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Liste over alle roller som brukeren har i systemet.
    /// For eksempel "Admin", "Registrar", "Pilot", "Entrepreneur", "DefaultUser" eller "OrgAdmin".
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// Liste over kortkodene til organisasjonene som brukeren tilhører.
    /// For eksempel "NLA", "LFS", "KRT". Hvis brukeren ikke tilhører noen organisasjoner, vil listen være tom.
    /// </summary>
    public List<string> Organizations { get; set; } = new List<string>();
}