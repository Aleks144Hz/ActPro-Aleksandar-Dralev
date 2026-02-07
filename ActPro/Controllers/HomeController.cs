using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Models;
using Microsoft.AspNetCore.Authorization;
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
            var approvedPlaces = await _context.Places
                .Include(p => p.PlaceImages)
                .Include(p => p.City)
                .Include(p => p.Activity)
                .Where(p => p.IsApproved == true)
                .ToListAsync();
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
        //--News--
        public async Task<IActionResult> News(int page = 1)
        {
            int pageSize = 3;
            var totalNews = await _context.News.CountAsync();

            var news = await _context.News
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalNews / pageSize);

            return View(news);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult CreateNews() => View();
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNews(News news, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/news", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    news.ImageURL = fileName;
                }

                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(News));
            }
            return View(news);
        }
        //--Delete News--
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var item = await _context.News.FindAsync(id);
            if (item != null)
            {
                _context.News.Remove(item);
                await _context.SaveChangesAsync();
            }
            return View();
        }
    }
}