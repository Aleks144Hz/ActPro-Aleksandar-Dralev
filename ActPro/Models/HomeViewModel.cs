using ActPro.DAL.Entities;

namespace ActPro.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Place> TopPlaces { get; set; }
        public List<string> CityNames { get; set; }
        public List<string> ActivityNames { get; set; }
    }
}