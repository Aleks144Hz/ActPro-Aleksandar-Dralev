using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "NameIsRequired")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastNameIsRequired")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "EmailIsRequired")]
        [EmailAddress(ErrorMessage = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PhoneNumberRequired")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "InvalidPhoneNumber")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "PasswordIsRequired")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "PasswordMismatch")]
        [Required(ErrorMessage = "ConfirmPasswordIsRequired")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "ProveYouAreNotRobot")]
        [BindProperty(Name = "g-recaptcha-response")]
        public string? CaptchaResponse { get; set; }

        public bool AcceptTerms { get; set; }

    }
}
