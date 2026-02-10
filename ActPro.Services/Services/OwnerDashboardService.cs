using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class OwnerDashboardService(IRepository<Place> placeRepo, IRepository<Reservation> resRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo) : IOwnerDashboardService
    {
        public async Task<OwnerDashboardViewModel> GetOwnerStatsAsync(string userId)
        {
            var model = new OwnerDashboardViewModel();
            model.MyPlaces = await placeRepo.AllAsNoTracking()
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .Where(p => p.OwnerId == userId).ToListAsync();

            var placeIds = model.MyPlaces.Select(p => p.Id).ToList();

            if (placeIds.Any())
            {
                model.RecentReservations = await resRepo.AllAsNoTracking()
                .Include(r => r.Place)
                .Where(r => placeIds.Contains((int)r.PlaceId))
                .OrderByDescending(r => r.CreatedAt).Take(10).ToListAsync();

                model.TotalIncome = (decimal)await resRepo.AllAsNoTracking()
                .Where(r => placeIds.Contains((int)r.PlaceId))
                .Join(placeRepo.AllAsNoTracking(), r => r.PlaceId, p => p.Id, (r, p) => p.Price)
                .SumAsync();

                model.TotalReservationsCount = await resRepo.AllAsNoTracking().CountAsync(r => placeIds.Contains((int)r.PlaceId));
            }
            else { model.RecentReservations = new List<Reservation>(); }

            return model;
        }
        public async Task<IEnumerable<City>> GetCitiesAsync() => await cityRepo.AllAsNoTracking().OrderBy(c => c.Name).ToListAsync();
        public async Task<IEnumerable<Activity>> GetActivitiesAsync() => await activityRepo.AllAsNoTracking().OrderBy(a => a.Name).ToListAsync();
    }
}
