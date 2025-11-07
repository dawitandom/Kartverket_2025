using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Brukernavn")]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Fornavn")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Etternavn")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Passord må være minst 12 tegn.")]
        [Display(Name = "Passord")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
        [Display(Name = "Bekreft passord")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}