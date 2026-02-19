using System.ComponentModel.DataAnnotations;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Domain.Models
{
    public class SupportTicketViewModel
    {
        [Required(ErrorMessage = EnterFirstName)]
        public string FullName { get; set; }

        [Required(ErrorMessage = EnterEmail)]
        [EmailAddress(ErrorMessage = Helpers.MessageConstants.Email)]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required(ErrorMessage = DescribeYourIssue)]
        [MinLength(10, ErrorMessage = IssueLenght)]
        public string Description { get; set; }
    }
}
