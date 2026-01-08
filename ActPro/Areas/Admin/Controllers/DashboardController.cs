using ActPro.DAL;
using ActPro.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalReservations = await _context.Reservations.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.PendingComments = await _context.Comments.CountAsync();
            ViewBag.TotalPlaces = await _context.Places.CountAsync();

            var latestReservations = await _context.Reservations
                .Include(r => r.Place)
                .Include(r => r.AspNetUser)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(latestReservations);
        }

        //---Chart Data Endpoint---
        [HttpGet]
        public async Task<IActionResult> GetChartData(string period = "week")
        {
            DateTime now = DateTime.Now;
            DateTime startDate = period switch
            {
                "week" => DateTime.Today.AddDays(-6),
                "month" => DateTime.Today.AddDays(-29),
                "year" => new DateTime(now.Year, 1, 1),
                _ => DateTime.Today.AddYears(-1)
            };

            var resData = await _context.Reservations.Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var userData = await _context.Users.Where(x => x.CreatedOn >= startDate)
                .GroupBy(x => x.CreatedOn.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var commData = await _context.Comments.Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Value.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

            var allDates = Enumerable.Range(0, (DateTime.Today - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).Date).ToList();

            var result = allDates.Select(d => new {
                date = d.ToString(period == "year" ? "MM.yyyy" : "dd.MM"),
                reservations = resData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                users = userData.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                comments = commData.FirstOrDefault(x => x.Date == d)?.Count ?? 0
            }).ToList();

            return Json(new
            {
                labels = result.Select(r => r.date),
                reservations = result.Select(r => r.reservations),
                users = result.Select(r => r.users),
                comments = result.Select(r => r.comments)
            });
        }
    }
}
