using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = EmailIsRequired)]
        [EmailAddress(ErrorMessage = "Грешен имейл")]
        public string Email { get; set; }

        [Required(ErrorMessage = PasswordIsRequired)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
