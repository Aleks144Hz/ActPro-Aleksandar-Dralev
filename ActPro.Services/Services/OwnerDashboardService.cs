using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class OwnerDashboardService : IOwnerDashboardService
    {
        private readonly IRepository<Place> _placeRepo;
        private readonly IRepository<Reservation> _resRepo;
        private readonly IRepository<City> _cityRepo;
        private readonly IRepository<Activity> _activityRepo;

        public OwnerDashboardService(IRepository<Place> placeRepo, IRepository<Reservation> resRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo)
        {
            _placeRepo = placeRepo;
            _resRepo = resRepo;
            _cityRepo = cityRepo;
            _activityRepo = activityRepo;
        }

        public async Task<OwnerDashboardViewModel> GetOwnerStatsAsync(string userId)
        {
            var model = new OwnerDashboardViewModel();
            model.MyPlaces = await _placeRepo.AllAsNoTracking()
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .Where(p => p.OwnerId == userId).ToListAsync();

            var placeIds = model.MyPlaces.Select(p => p.Id).ToList();

            if (placeIds.Any())
            {
                model.RecentReservations = await _resRepo.AllAsNoTracking()
                .Include(r => r.Place)
                .Where(r => placeIds.Contains((int)r.PlaceId))
                .OrderByDescending(r => r.CreatedAt).Take(10).ToListAsync();

                model.TotalIncome = (decimal)await _resRepo.AllAsNoTracking()
                .Where(r => placeIds.Contains((int)r.PlaceId))
                .Join(_placeRepo.AllAsNoTracking(), r => r.PlaceId, p => p.Id, (r, p) => p.Price)
                .SumAsync();

                model.TotalReservationsCount = await _resRepo.AllAsNoTracking().CountAsync(r => placeIds.Contains((int)r.PlaceId));
            }
            else { model.RecentReservations = new List<Reservation>(); }

            return model;
        }
        public async Task<IEnumerable<City>> GetCitiesAsync() => await _cityRepo.AllAsNoTracking().OrderBy(c => c.Name).ToListAsync();
        public async Task<IEnumerable<Activity>> GetActivitiesAsync() => await _activityRepo.AllAsNoTracking().OrderBy(a => a.Name).ToListAsync();
    }
}
