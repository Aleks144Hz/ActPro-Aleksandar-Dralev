using ActPro.DAL.Entities;

namespace ActPro.Services.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<Place>> SearchPlacesAsync(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice, string sortOrder, string capacityGroup);
        Task<List<string>> GetCityNamesAsync();
        Task<List<string>> GetActivityNamesAsync();
        Task<List<City>> GetAllCitiesAsync();
        Task<List<Activity>> GetAllActivitiesAsync();
        Task<Dictionary<string, int>> GetSearchStatsAsync(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice);
    }
}