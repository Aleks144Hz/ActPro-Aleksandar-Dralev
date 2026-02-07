using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Models.User
{
    public class DeleteProfileViewModel
    {
        [Required(ErrorMessage = PasswordIsRequired)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
