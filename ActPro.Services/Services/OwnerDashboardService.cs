using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class OwnerDashboardService(IRepository<Place> placeRepo, IRepository<Reservation> resRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo) : IOwnerDashboardService
    {
        //--- Owner Dashboard Stats ---
        public async Task<OwnerDashboardViewModel> GetOwnerStatsAsync(string userId)
        {
            var model = new OwnerDashboardViewModel();

            model.MyPlaces = await placeRepo.AllAsNoTracking()
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .Where(p => p.OwnerId == userId)
            .ToListAsync();

            var placeIds = model.MyPlaces.Select(p => p.Id).ToList();

            var cities = await GetCitiesAsync();
            var activities = await GetActivitiesAsync();
            var cityOptions = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            var activityOptions = activities.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();

            if (placeIds.Any())
            {
                model.RecentReservations = await resRepo.AllAsNoTracking()
                .Include(r => r.Place)
                .Where(r => r.PlaceId != null && placeIds.Contains(r.PlaceId.Value))
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

                var total = await resRepo.AllAsNoTracking()
                .Where(r => r.PlaceId != null && placeIds.Contains(r.PlaceId.Value))
                .Join(placeRepo.AllAsNoTracking(), r => r.PlaceId, p => p.Id, (r, p) => p.Price)
                .SumAsync();

                model.TotalIncome = (decimal)total;
                model.TotalReservationsCount = await resRepo.AllAsNoTracking()
                .CountAsync(r => r.PlaceId != null && placeIds.Contains(r.PlaceId.Value));

                foreach (var place in model.MyPlaces)
                {
                    model.EditPlaces[place.Id] = new PlaceFormViewModel
                    {
                        Id = place.Id,
                        Name = place.Name,
                        Address = place.Address ?? "",
                        Price = place.Price,
                        Capacity = place.Capacity,
                        Description = place.Description,
                        IsOutdoor = place.IsOutdoor,
                        CityId = place.CityId ?? 0,
                        ActivityId = place.ActivityId ?? 0,
                        CityOptions = cityOptions,
                        ActivityOptions = activityOptions,
                        OwnerId = place.OwnerId,
                        Rating = place.Rating,
                        ExistingImages = place.PlaceImages?.Select(img => new PlaceImageViewModel
                        {
                            Id = img.Id,
                            Url = img.ImageUrl
                        }).ToList() ?? new List<PlaceImageViewModel>()
                    };

                    model.PlaceSchedules[place.Id] = new PlaceScheduleViewModel
                    {
                        PlaceId = place.Id,
                        PlaceName = place.Name,
                        Closures = place.PlaceClosures?.Select(c => new ClosureViewModel
                        {
                            Id = c.Id,
                            StartDate = c.ClosureDate,
                            EndDate = c.ClosureDate,
                            Reason = c.Reason
                        }).ToList() ?? new List<ClosureViewModel>()
                    };
                }
            }
            return model;
        }
        public async Task<IEnumerable<City>> GetCitiesAsync() => await cityRepo.AllAsNoTracking().OrderBy(c => c.Name).ToListAsync();
        public async Task<IEnumerable<Activity>> GetActivitiesAsync() => await activityRepo.AllAsNoTracking().OrderBy(a => a.Name).ToListAsync();
    }
}
