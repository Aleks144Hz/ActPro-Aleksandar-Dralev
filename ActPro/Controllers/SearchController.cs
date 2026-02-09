using ActPro.Models;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        //--- SEARCH PAGE ---
        public async Task<IActionResult> Index(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice, string sortOrder, string capacityGroup)
        {
            var results = await _searchService.SearchPlacesAsync(city, activity, isOutdoor, minPrice, maxPrice, sortOrder, capacityGroup);

            var stats = await _searchService.GetSearchStatsAsync(city, activity, isOutdoor, minPrice, maxPrice);

            var viewModel = new SearchViewModel
            {
                Results = results,
                City = city,
                Activity = activity,
                IsOutdoor = isOutdoor,
                MinPrice = minPrice ?? 0,
                MaxPrice = maxPrice ?? 200,
                SortOrder = sortOrder,
                CapacityGroup = capacityGroup,
                CitiesList = await _searchService.GetCityNamesAsync(),
                ActivitiesList = await _searchService.GetActivityNamesAsync(),
                SmallCount = stats.GetValueOrDefault("SmallCount", 0),
                MediumCount = stats.GetValueOrDefault("MediumCount", 0),
                LargeCount = stats.GetValueOrDefault("LargeCount", 0),
                OutdoorCount = stats.GetValueOrDefault("OutdoorCount", 0),
                IndoorCount = stats.GetValueOrDefault("IndoorCount", 0)
            };

            return View(viewModel);
        }
    }
}