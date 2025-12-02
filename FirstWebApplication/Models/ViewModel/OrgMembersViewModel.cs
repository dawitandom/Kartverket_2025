using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// Data for å vise medlemmer i en organisasjon.
/// </summary>
public class OrgMembersViewModel
{
    /// <summary>
    /// Organisasjons-ID.
    /// </summary>
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// Organisasjonsnavn.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Liste over medlemmer.
    /// </summary>
    public List<OrgMemberDto> Members { get; set; } = new();
}

/// <summary>
/// Data om et organisasjonsmedlem.
/// </summary>
public class OrgMemberDto
{
    /// <summary>
    /// Bruker-ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukernavn.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// E-postadresse.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Fullt navn (fornavn + etternavn).
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukerens roller (kommaseparert).
    /// </summary>
    public string Roles { get; set; } = string.Empty;
}