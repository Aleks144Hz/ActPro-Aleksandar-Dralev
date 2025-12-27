using ActPro.DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string city, string activity)
        {
            var query = _context.Places
                .Include(p => p.City)
                .Include(p => p.PlaceImages)
                .AsQueryable();
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(p => p.City.Name == city);
            }
            if (!string.IsNullOrEmpty(activity))
            {
                query = query.Where(p => p.Activity.Name == activity);
            }
            var results = await query.ToListAsync();
            return View("Search", results);
        }
    }
}