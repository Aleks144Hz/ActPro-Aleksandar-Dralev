using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Грешен имейл")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Паролата е задължителна")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
