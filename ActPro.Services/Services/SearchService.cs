using ActPro.DAL.Entities;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services
{
    public class SearchService(IRepository<Place> placeRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo) : ISearchService
    {
        //-- SEARCH & FILTERING
        public async Task<IEnumerable<Place>> SearchPlacesAsync(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice, string sortOrder, string capacityGroup)
        {
            var query = BuildBaseQuery(city, activity, isOutdoor, minPrice, maxPrice);

            if (!string.IsNullOrEmpty(capacityGroup))
            {
                query = capacityGroup switch
                {
                    "small" => query.Where(p => p.Capacity >= 1 && p.Capacity <= 4),
                    "medium" => query.Where(p => p.Capacity >= 5 && p.Capacity <= 14),
                    "large" => query.Where(p => p.Capacity >= 15),
                    _ => query
                };
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating_des" => query.OrderByDescending(p => p.Rating),
                "rating_asc" => query.OrderBy(p => p.Rating),
                _ => query.OrderBy(p => p.Name)
            };

            return await query.ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetSearchStatsAsync(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice)
        {
            var query = BuildBaseQuery(city, activity, isOutdoor, minPrice, maxPrice);

            return new Dictionary<string, int>
            {
                { "SmallCount", await query.CountAsync(p => p.Capacity >= 1 && p.Capacity <= 4) },
                { "MediumCount", await query.CountAsync(p => p.Capacity >= 5 && p.Capacity <= 14) },
                { "LargeCount", await query.CountAsync(p => p.Capacity >= 15) },
                { "OutdoorCount", await query.CountAsync(p => p.IsOutdoor == true) },
                { "IndoorCount", await query.CountAsync(p => p.IsOutdoor == false) }
            };
        }

        private IQueryable<Place> BuildBaseQuery(string city, string activity, bool? isOutdoor, decimal? minPrice, decimal? maxPrice)
        {
            var query = placeRepo.AllAsNoTracking()
                .Include(p => p.City)
                .Include(p => p.Activity)
                .Include(p => p.PlaceImages)
                .Where(p => p.IsApproved);

            var isEnglish = System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en";

            if (!string.IsNullOrEmpty(city))
            {
                if (isEnglish)
                    query = query.Where(p => p.City.NameEn == city || p.City.Name == city);
                else
                    query = query.Where(p => p.City.Name == city);
            }

            if (!string.IsNullOrEmpty(activity))
            {
                if (isEnglish)
                    query = query.Where(p => p.Activity.NameEn == activity || p.Activity.Name == activity);
                else
                    query = query.Where(p => p.Activity.Name == activity);
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (isOutdoor.HasValue) query = query.Where(p => p.IsOutdoor == isOutdoor.Value);

            return query;
        }

        public async Task<List<string>> GetCityNamesAsync()
        {
            var cities = await cityRepo.AllAsNoTracking().ToListAsync();
            var isEnglish = System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en";
            return cities.Select(c => isEnglish && !string.IsNullOrEmpty(c.NameEn) ? c.NameEn : c.Name).ToList();
        }

        public async Task<List<string>> GetActivityNamesAsync()
        {
            var activities = await activityRepo.AllAsNoTracking().ToListAsync();
            var isEnglish = System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en";
            return activities.Select(a => isEnglish && !string.IsNullOrEmpty(a.NameEn) ? a.NameEn : a.Name).ToList();
        }

        public async Task<List<City>> GetAllCitiesAsync() => await cityRepo.AllAsNoTracking().ToListAsync();
        public async Task<List<Activity>> GetAllActivitiesAsync() => await activityRepo.AllAsNoTracking().ToListAsync();
    }
}