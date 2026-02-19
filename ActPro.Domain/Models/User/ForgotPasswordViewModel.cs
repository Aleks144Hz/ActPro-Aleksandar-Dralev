using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;
namespace ActPro.Domain.Models.Account
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = NewPasswordIsRequired)]
        public string Password { get; set; }

        [Required(ErrorMessage = ConfirmPasswordIsRequired)]
        [Compare("Password", ErrorMessage = PasswordMismatch)]
        public string ConfirmPassword { get; set; }
    }
}