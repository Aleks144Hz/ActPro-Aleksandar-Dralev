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

        [Required(ErrorMessage = PhoneNumberRequired)]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = InvalidPhoneNumber)]
        public string PhoneNumber { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public string? ExistingPicturePath { get; set; }
    }
}
