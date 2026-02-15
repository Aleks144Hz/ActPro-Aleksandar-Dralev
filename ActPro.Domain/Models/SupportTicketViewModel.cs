using System.ComponentModel.DataAnnotations;

namespace ActPro.Domain.Models
{
    public class SupportTicketViewModel
    {
        [Required(ErrorMessage = "Моля, въведете вашето име.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; } // Тип проблем

        [Required(ErrorMessage = "Моля, опишете проблема си.")]
        [MinLength(10, ErrorMessage = "Описанието трябва да е поне 10 символа.")]
        public string Description { get; set; }
    }
}
