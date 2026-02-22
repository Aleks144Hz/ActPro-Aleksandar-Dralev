using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "EmailIsRequired")]
        [EmailAddress(ErrorMessage = "WrongEmail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PasswordIsRequired")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
