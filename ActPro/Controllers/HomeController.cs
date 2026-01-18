using ActPro.DAL.Data;
using ActPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topPlaces = await _context.Places
                .Include(p => p.City)
                .Include(p => p.PlaceImages)
                .OrderByDescending(p => p.Rating)
                .Take(3)
                .ToListAsync();
            var cities = await _context.Cities
                .Select(c => c.Name)
                .Distinct()
                .ToListAsync();
            var activities = await _context.Activities
                .Select(a => a.Name)
                .Distinct()
                .ToListAsync();
            var sportCounts = await _context.Places
                .GroupBy(p => p.Activity.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);
            var viewModel = new HomeViewModel
            {
                TopPlaces = topPlaces,
                CityNames = cities,
                ActivityNames = activities,
                SportCounts = sportCounts
            };
            return View(viewModel);
        }
    }
}