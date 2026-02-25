using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ActPro.Domain.Models.Owner
{
    public class PlaceEntryViewModel
    {
        [Required(ErrorMessage = "ValidationNameRequired")]
        [StringLength(200, ErrorMessage = "ValidationNameTooLong")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ValidationAddressRequired")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "ValidationDescriptionRequired")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "ValidationPriceRequired")]
        [Range(0.01, 9999, ErrorMessage = "ValidationPriceRange")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "ValidationCapacityRequired")]
        [Range(1, 1000, ErrorMessage = "ValidationCapacityRange")]
        public int? Capacity { get; set; }

        [Required(ErrorMessage = "ValidationTypeRequired")]
        public bool? IsOutdoor { get; set; }

        [Required(ErrorMessage = "ValidationCityRequired")]
        public int? CityId { get; set; }

        [Required(ErrorMessage = "ValidationActivityRequired")]
        public int? ActivityId { get; set; }

        public IEnumerable<IFormFile>? ImageFiles { get; set; }
    }
}