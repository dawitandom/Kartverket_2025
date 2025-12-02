using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// ApplicationUser utvider IdentityUser med egendefinerte felt.
/// Lagres i AspNetUsers-tabellen. Inneholder brukerens navn og lenker til rapporter og organisasjoner.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Navigasjonsegenskap: Alle rapporter som er opprettet av denne brukeren.
    /// Entity Framework bruker dette til å håndtere relasjonen mellom bruker og rapporter.
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    
    /// <summary>
    /// Beregnet egenskap for bakoverkompatibilitet.
    /// Returnerer UserName fra Identity.
    /// </summary>
    public string Username => UserName ?? string.Empty;
    
    /// <summary>
    /// Organisasjoner som denne brukeren tilhører (mange-til-mange via OrganizationUser).
    /// En bruker kan tilhøre flere organisasjoner, og en organisasjon kan ha mange brukere.
    /// </summary>
    public ICollection<OrganizationUser> Organizations { get; set; } = new List<OrganizationUser>();
}