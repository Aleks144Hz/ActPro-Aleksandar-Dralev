using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ActPro.Services.Services
{
    public class AdminDashboardService(IRepository<Reservation> resRepo, IRepository<ApplicationUser> userRepo, IRepository<Comment> commRepo, IRepository<Place> placeRepo) : IAdminDashboardService
    {
        //--- Admin Dashboard Stats
        public async Task<AdminDashboardViewModel> GetAdminStatsAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalReservations = await resRepo.AllAsNoTracking().CountAsync(),
                TotalUsers = await userRepo.AllAsNoTracking().CountAsync(),
                PendingComments = await commRepo.AllAsNoTracking().CountAsync(),
                TotalPlaces = await placeRepo.AllAsNoTracking().CountAsync(),
                LatestReservations = await resRepo.AllAsNoTracking()
                .Include(r => r.Place)
                .Include(r => r.AspNetUser)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync()
            };
        }

        public async Task<object> GetChartDataAsync(string period)
        {
            DateTime now = DateTime.Now;
            DateTime startDate = period switch
            {
                "week" => DateTime.Today.AddDays(-6),
                "month" => DateTime.Today.AddDays(-29),
                "year" => new DateTime(now.Year, 1, 1),
                _ => DateTime.Today.AddYears(-1)
            };

            var resData = await resRepo.AllAsNoTracking().Where(x => x.CreatedAt >= startDate).GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var userData = await userRepo.AllAsNoTracking().Where(x => x.CreatedOn >= startDate).GroupBy(x => x.CreatedOn.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var commData = await commRepo.AllAsNoTracking().Where(x => x.CreatedAt >= startDate).GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var allDates = Enumerable.Range(0, (DateTime.Today - startDate).Days + 1).Select(offset => startDate.AddDays(offset).Date).ToList();

            var result = allDates.Select(d => new
            {
                date = d.ToString(period == "year" ? "MM.yyyy" : "dd.MM", CultureInfo.InvariantCulture),
                reservations = resData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                users = userData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                comments = commData.FirstOrDefault(x => x.Date == d)?.Count ?? 0
            }).ToList();

            return new
            {
                labels = result.Select(r => r.date).ToList(),
                reservations = result.Select(r => r.reservations).ToList(),
                users = result.Select(r => r.users).ToList(),
                comments = result.Select(r => r.comments).ToList()
            };
        }
    }
}
