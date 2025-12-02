using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models;

/// <summary>
/// ViewModel for å opprette en organisasjonsadministrator.
/// Brukes i OrganizationsController når en administrator oppretter en ny organisasjonsadministrator for en organisasjon.
/// </summary>
public class CreateOrgAdminViewModel
{
    /// <summary>
    /// ID-en til organisasjonen som den nye organisasjonsadministratoren skal tilhøre.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Navnet på organisasjonen som den nye organisasjonsadministratoren skal tilhøre.
    /// Brukes kun for visning i skjemaet.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Brukernavnet som skal brukes for den nye organisasjonsadministratoren.
    /// Må være unikt i systemet. Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-postadressen til den nye organisasjonsadministratoren.
    /// Må være en gyldig e-postadresse og unik i systemet. Påkrevd felt.
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Fornavnet til den nye organisasjonsadministratoren.
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Etternavnet til den nye organisasjonsadministratoren.
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Passordet som skal brukes for den nye organisasjonsadministratoren.
    /// Må være minst 6 tegn langt. Påkrevd felt.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Bekreftelse av passordet. Må matche passord-feltet.
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}