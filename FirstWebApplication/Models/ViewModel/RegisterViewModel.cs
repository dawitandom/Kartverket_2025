using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// Data for å registrere en ny bruker.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Brukernavn.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username cannot be longer than 100 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Fornavn.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Etternavn.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// E-postadresse.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Passord (minst 12 tegn).
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Bekreft passord.
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}