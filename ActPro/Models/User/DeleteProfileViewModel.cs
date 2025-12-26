using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class DeleteProfileViewModel
    {
        [Required(ErrorMessage = "Паролата е задължителна за изтриване на профила.")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }
    }
}
