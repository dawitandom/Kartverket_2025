using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for selvbetjent registrering av nye brukere.
    /// Brukes i AccountController når nye brukere oppretter en konto selv.
    /// Alle nye brukere får automatisk rollen "DefaultUser" ved registrering.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Brukernavnet som skal brukes for den nye kontoen.
        /// Må være unikt i systemet. Påkrevd felt, maksimalt 100 tegn.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username cannot be longer than 100 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Fornavnet til den nye brukeren.
        /// Påkrevd felt, maksimalt 100 tegn.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Etternavnet til den nye brukeren.
        /// Påkrevd felt, maksimalt 100 tegn.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// E-postadressen til den nye brukeren.
        /// Må være en gyldig e-postadresse og unik i systemet. Påkrevd felt.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Passordet som skal brukes for den nye kontoen.
        /// Må være minst 12 tegn langt for sikkerhet. Påkrevd felt.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Bekreftelse av passordet. Må matche passord-feltet eksakt.
        /// Påkrevd felt.
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}