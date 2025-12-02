using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models;

/// <summary>
/// ViewModel for å opprette en ny bruker i systemet.
/// Brukes i AdminController når en administrator oppretter en ny bruker med en spesifikk rolle.
/// </summary>
public class CreateUserViewModel
{
    /// <summary>
    /// Brukernavnet som skal brukes for den nye brukeren.
    /// Må være unikt i systemet. Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-postadressen til den nye brukeren.
    /// Må være en gyldig e-postadresse og unik i systemet. Påkrevd felt.
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Fornavnet til den nye brukeren.
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Etternavnet til den nye brukeren.
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Passordet som skal brukes for den nye brukeren.
    /// Må være minst 6 tegn langt. Påkrevd felt.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Bekreftelse av passordet. Må matche passord-feltet.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Rollen som skal tildeles den nye brukeren.
    /// For eksempel "Admin", "Registrar", "Pilot", "Entrepreneur", "DefaultUser" eller "OrgAdmin".
    /// Påkrevd felt.
    /// </summary>
    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;
}