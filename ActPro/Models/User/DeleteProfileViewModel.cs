using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class DeleteProfileViewModel
    {
        [Required(ErrorMessage = "Паролата е задължителна.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
