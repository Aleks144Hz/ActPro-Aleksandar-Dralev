using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            var activities = await _context.Activities.OrderBy(a => a.Name).ToListAsync();

            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(activities, "Id", "Name");

            var model = new OwnerDashboardViewModel();

            model.MyPlaces = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Activity)
                .Include(p => p.PlaceImages)
                .Include(p => p.PlaceClosures)
                .Where(p => p.OwnerId == userId)
                .ToListAsync();

            var placeIds = model.MyPlaces.Select(p => p.Id).ToList();

            if (placeIds.Any())
            {
                model.RecentReservations = await _context.Reservations
                    .Include(r => r.Place)
                    .Where(r => placeIds.Contains((int)r.PlaceId))
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                model.TotalIncome = (decimal)await _context.Reservations
                    .Where(r => placeIds.Contains((int)r.PlaceId))
                    .Join(_context.Places, r => r.PlaceId, p => p.Id, (r, p) => p.Price)
                    .SumAsync();

                model.TotalReservationsCount = await _context.Reservations
                    .CountAsync(r => placeIds.Contains((int)r.PlaceId));
            }

            if (model.RecentReservations == null) model.RecentReservations = new List<Reservation>();
            return View(model);
        }
    }
}