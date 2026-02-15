using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Новата парола е задължителна.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Потвърждението на новата парола е задължителна.")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }
    }
}