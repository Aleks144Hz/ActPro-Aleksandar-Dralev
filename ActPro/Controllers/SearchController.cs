using ActPro.DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        //--- SEARCH PAGE ---
        public async Task<IActionResult> Index(string city, string activity, decimal? minPrice, decimal? maxPrice, string sortOrder, string capacityGroup)
        {
            var query = _context.Places.Include(p => p.City).Include(p => p.Activity).Include(p => p.PlaceImages).AsQueryable();
            ViewBag.CitiesList = await _context.Cities.Select(c => c.Name).ToListAsync();
            ViewBag.ActivitiesList = await _context.Activities.Select(a => a.Name).ToListAsync();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(p => p.City.Name == city);
            }
            if (!string.IsNullOrEmpty(activity))
            {
                query = query.Where(p => p.Activity.Name == activity);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            ViewBag.SmallCount = await query.CountAsync(p => p.Capacity >= 1 && p.Capacity <= 4);
            ViewBag.MediumCount = await query.CountAsync(p => p.Capacity >= 5 && p.Capacity <= 14);
            ViewBag.LargeCount = await query.CountAsync(p => p.Capacity >= 15);
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Activities = await _context.Activities.ToListAsync();

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
            ViewBag.CurrentCity = city;
            ViewBag.CurrentActivity = activity;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCapacity = capacityGroup;

            var results = await query.ToListAsync();
            return View("Search", results);
        }
    }
}