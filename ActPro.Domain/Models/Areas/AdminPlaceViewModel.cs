using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActPro.Domain.Models.Areas
{
    public class PlacesIndexViewModel
    {
        public List<PlaceViewModel> Places { get; set; } = new();
        public IEnumerable<SelectListItem> CityOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ActivityOptions { get; set; } = new List<SelectListItem>();
        public Dictionary<int, PlaceFormViewModel> EditPlaces { get; set; } = new();
        public Dictionary<int, PlaceScheduleViewModel> PlaceSchedules { get; set; } = new();
    }

    public class PlaceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Description { get; set; }
        public string? DescriptionEn { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? CityNameEn { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string? ActivityNameEn { get; set; }
        public decimal Price { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOutdoor { get; set; }
        public int CityId { get; set; }
        public int ActivityId { get; set; }
        public List<PlaceImageViewModel> ExistingImages { get; set; } = new();
    }

    public class PlaceFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool? IsOutdoor { get; set; }
        public int CityId { get; set; }
        public int ActivityId { get; set; }
        public decimal? Price { get; set; }
        public int? Capacity { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public string? OwnerId { get; set; }
        public int? Rating { get; set; }
        public List<PlaceImageViewModel> ExistingImages { get; set; } = new();
        public IEnumerable<SelectListItem> CityOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ActivityOptions { get; set; } = new List<SelectListItem>();
    }

    public class PlaceScheduleViewModel
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string? PlaceNameEn { get; set; }
        public List<ClosureViewModel> Closures { get; set; } = new();
    }

    public class PlaceImageViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class ClosureViewModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}