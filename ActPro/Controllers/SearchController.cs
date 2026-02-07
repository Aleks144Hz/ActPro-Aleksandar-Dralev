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
            ViewBag.CitiesList = await _searchService.GetCityNamesAsync();
            ViewBag.ActivitiesList = await _searchService.GetActivityNamesAsync();
            ViewBag.Cities = await _searchService.GetAllCitiesAsync();
            ViewBag.Activities = await _searchService.GetAllActivitiesAsync();

            var stats = await _searchService.GetSearchStatsAsync(city, activity, isOutdoor, minPrice, maxPrice);
            ViewBag.SmallCount = stats["SmallCount"];
            ViewBag.MediumCount = stats["MediumCount"];
            ViewBag.LargeCount = stats["LargeCount"];
            ViewBag.OutdoorCount = stats["OutdoorCount"];
            ViewBag.IndoorCount = stats["IndoorCount"];
            ViewBag.CurrentCity = city;
            ViewBag.CurrentActivity = activity;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCapacity = capacityGroup;

            var results = await _searchService.SearchPlacesAsync(city, activity, isOutdoor, minPrice, maxPrice, sortOrder, capacityGroup);

            return View("Index", results);
        }
    }
}