using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "CurrentPasswordIsRequired")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "NewPasswordIsRequired")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmPasswordIsRequired")]
        [Compare("NewPassword", ErrorMessage = "PasswordMismatch")]
        public string ConfirmPassword { get; set; }
    }
}
