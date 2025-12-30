using System.ComponentModel.DataAnnotations;

namespace ActPro.Models.User
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Името е задължително")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}
