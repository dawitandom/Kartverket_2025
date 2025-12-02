using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// Brukermodell som utvider IdentityUser med egne felt.
/// Lagrer brukerinformasjon som fornavn, etternavn og koblinger til rapporter og organisasjoner.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Brukerens fornavn.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Brukerens etternavn.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Alle rapporter som denne brukeren har laget.
    /// En bruker kan ha mange rapporter.
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    
    /// <summary>
    /// Brukernavn som egenskap (for bakoverkompatibilitet med gammel kode).
    /// Returnerer samme verdi som UserName fra Identity.
    /// </summary>
    public string Username => UserName ?? string.Empty;
    
    /// <summary>
    /// Organisasjoner som denne brukeren tilhører.
    /// En bruker kan tilhøre flere organisasjoner gjennom OrganizationUser-tabellen.
    /// </summary>
    public ICollection<OrganizationUser> Organizations { get; set; } = new List<OrganizationUser>();
}