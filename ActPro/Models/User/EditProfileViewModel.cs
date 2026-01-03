using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Models.User
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = NameIsRequired)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = LastNameIsRequired)]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}
