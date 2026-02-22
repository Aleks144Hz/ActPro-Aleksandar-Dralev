using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class DeleteProfileViewModel
    {
        [Required(ErrorMessage = "PasswordIsRequired")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
