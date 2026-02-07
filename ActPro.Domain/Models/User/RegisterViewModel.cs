using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Models.User
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = NameIsRequired)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = LastNameIsRequired)]
        public string LastName { get; set; }

        [Required(ErrorMessage = EmailIsRequired)]
        [EmailAddress(ErrorMessage = Helpers.MessageConstants.Email)]
        public string Email { get; set; }

        [Required(ErrorMessage = PhoneNumberRequired)]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = InvalidPhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = PasswordIsRequired)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = PasswordMismatch)]
        [Required(ErrorMessage = ConfirmPasswordIsRequired)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Моля потвърдете, че не сте робот.")]
        [BindProperty(Name = "g-recaptcha-response")]
        public string? CaptchaResponse { get; set; }
    }
}
