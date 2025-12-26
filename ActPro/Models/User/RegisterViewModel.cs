using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Името е задължително")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл формат")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефонен номер е задължителен")]
        [Phone(ErrorMessage = "Невалиден телефонен номер")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Паролата е задължителна")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]

        [Required (ErrorMessage = "Паролата за потвърждение е задължителна")]
        public string ConfirmPassword { get; set; }
    }
}
