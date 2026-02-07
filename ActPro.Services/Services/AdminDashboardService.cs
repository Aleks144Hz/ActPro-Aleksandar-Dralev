using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IRepository<Reservation> _resRepo;
        private readonly IRepository<ApplicationUser> _userRepo;
        private readonly IRepository<Comment> _commRepo;
        private readonly IRepository<Place> _placeRepo;

        public AdminDashboardService(IRepository<Reservation> resRepo, IRepository<ApplicationUser> userRepo, IRepository<Comment> commRepo, IRepository<Place> placeRepo)
        {
            _resRepo = resRepo;
            _userRepo = userRepo;
            _commRepo = commRepo;
            _placeRepo = placeRepo;
        }

        public async Task<AdminDashboardViewModel> GetAdminStatsAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalReservations = await _resRepo.AllAsNoTracking().CountAsync(),
                TotalUsers = await _userRepo.AllAsNoTracking().CountAsync(),
                PendingComments = await _commRepo.AllAsNoTracking().CountAsync(),
                TotalPlaces = await _placeRepo.AllAsNoTracking().CountAsync(),
                LatestReservations = await _resRepo.AllAsNoTracking()
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

            var resData = await _resRepo.AllAsNoTracking().Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var userData = await _userRepo.AllAsNoTracking().Where(x => x.CreatedOn >= startDate)
                .GroupBy(x => x.CreatedOn.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var commData = await _commRepo.AllAsNoTracking().Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var allDates = Enumerable.Range(0, (DateTime.Today - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).Date).ToList();

            var result = allDates.Select(d => new
            {
                date = d.ToString(period == "year" ? "MM.yyyy" : "dd.MM"),
                reservations = resData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                users = userData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                comments = commData.FirstOrDefault(x => x.Date == d)?.Count ?? 0
            }).ToList();

            return new
            {
                labels = result.Select(r => r.date),
                reservations = result.Select(r => r.reservations),
                users = result.Select(r => r.users),
                comments = result.Select(r => r.comments)
            };
        }
    }
}
