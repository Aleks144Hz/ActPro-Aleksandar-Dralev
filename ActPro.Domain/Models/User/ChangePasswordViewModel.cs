using System.ComponentModel.DataAnnotations;
namespace ActPro.Models.User
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Текущата парола е задължителна.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Новата парола е задължителна.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Потвърждението на новата парола е задължителна.")]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }
    }
}
