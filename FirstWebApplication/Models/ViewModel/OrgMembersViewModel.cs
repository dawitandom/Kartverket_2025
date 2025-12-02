using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// ViewModel for å vise alle medlemmer i en organisasjon.
/// Brukes i OrgAdminController for å vise en liste over alle brukere som tilhører organisasjonen.
/// </summary>
public class OrgMembersViewModel
{
    /// <summary>
    /// ID-en til organisasjonen som medlemmene tilhører.
    /// </summary>
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// Navnet på organisasjonen som medlemmene tilhører.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Liste over alle medlemmer i organisasjonen.
    /// Hver medlem inneholder informasjon om bruker-ID, brukernavn, e-post, navn og roller.
    /// </summary>
    public List<OrgMemberDto> Members { get; set; } = new();
}

/// <summary>
/// Data Transfer Object (DTO) som inneholder informasjon om et organisasjonsmedlem.
/// Brukes for å vise medlemsinformasjon i organisasjonsadministrasjonssiden.
/// </summary>
public class OrgMemberDto
{
    /// <summary>
    /// ID-en til brukeren som er medlem av organisasjonen.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukernavnet til medlemmet.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// E-postadressen til medlemmet.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Fullt navn på medlemmet (fornavn + etternavn).
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukerens roller i systemet, formatert som en kommaseparert streng.
    /// For eksempel "Pilot, DefaultUser" eller "—" hvis brukeren ikke har noen roller.
    /// </summary>
    public string Roles { get; set; } = string.Empty;
}