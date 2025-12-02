using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FirstWebApplication.Models;

/// <summary>
/// ApplicationUser extends IdentityUser with custom fields.
/// Stored in AspNetUsers table.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property: All reports created by this user.
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    
    /// <summary>
    /// Computed property for backwards compatibility.
    /// Returns UserName from Identity.
    /// </summary>
    public string Username => UserName ?? string.Empty;
    
    /// <summary>
    /// Organizations this user belongs to (many-to-many via OrganizationUser).
    /// </summary>
    public ICollection<OrganizationUser> Organizations { get; set; } = new List<OrganizationUser>();
}