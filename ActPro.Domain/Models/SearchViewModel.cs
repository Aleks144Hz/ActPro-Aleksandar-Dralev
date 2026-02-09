using ActPro.DAL.Entities;

namespace ActPro.Models
{
    public class SearchViewModel
    {
        public IEnumerable<Place> Results { get; set; } = new List<Place>();
        public string? City { get; set; }
        public string? Activity { get; set; }
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 200;
        public string? SortOrder { get; set; }
        public string? CapacityGroup { get; set; }
        public bool? IsOutdoor { get; set; }
        public List<string> CitiesList { get; set; } = new();
        public List<string> ActivitiesList { get; set; } = new();
        public int SmallCount { get; set; }
        public int MediumCount { get; set; }
        public int LargeCount { get; set; }
        public int OutdoorCount { get; set; }
        public int IndoorCount { get; set; }
    }
}