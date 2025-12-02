using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// Data for å vise og administrere brukere.
/// </summary>
public class UserManagementViewModel
{
    /// <summary>
    /// Bruker-ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukernavn.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// E-postadresse.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Fornavn.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Etternavn.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Liste over brukerens roller.
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// Liste over organisasjonskortkoder brukeren tilhører.
    /// </summary>
    public List<string> Organizations { get; set; } = new List<string>();
}