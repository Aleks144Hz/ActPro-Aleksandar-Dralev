using Microsoft.AspNetCore.Http;

namespace ActPro.Domain.Models.Owner
{
    public class PlaceEntryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Capacity { get; set; }
        public bool? IsOutdoor { get; set; }
        public int CityId { get; set; }
        public int ActivityId { get; set; }
        public IEnumerable<IFormFile>? ImageFiles { get; set; }
    }
}