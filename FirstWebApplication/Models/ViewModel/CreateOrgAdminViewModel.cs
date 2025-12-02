using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models;

/// <summary>
/// Data for å lage en OrgAdmin-bruker.
/// </summary>
public class CreateOrgAdminViewModel
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
    /// Brukernavn.
    /// </summary>
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-postadresse.
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Fornavn.
    /// </summary>
    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Etternavn.
    /// </summary>
    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Passord (minst 6 tegn).
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Bekreft passord.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}