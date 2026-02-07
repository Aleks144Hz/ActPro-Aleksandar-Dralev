using ActPro.DAL.Entities;
using ActPro.Domain.Repository;
using ActPro.Models;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services
{
    public class HomeService : IHomeService
    {
        private readonly IRepository<Place> _placeRepo;
        private readonly IRepository<City> _cityRepo;
        private readonly IRepository<Activity> _activityRepo;
        private readonly IRepository<News> _newsRepo;

        public HomeService(
            IRepository<Place> placeRepo,
            IRepository<City> cityRepo,
            IRepository<Activity> activityRepo,
            IRepository<News> newsRepo)
        {
            _placeRepo = placeRepo;
            _cityRepo = cityRepo;
            _activityRepo = activityRepo;
            _newsRepo = newsRepo;
        }

        public async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            var topPlaces = await _placeRepo.AllAsNoTracking()
                .Include(p => p.City)
                .Include(p => p.PlaceImages)
                .OrderByDescending(p => p.Rating)
                .Take(3)
                .ToListAsync();

            var cities = await _cityRepo.AllAsNoTracking()
                .Select(c => c.Name)
                .Distinct()
                .ToListAsync();

            var activities = await _activityRepo.AllAsNoTracking()
                .Select(a => a.Name)
                .Distinct()
                .ToListAsync();

            var sportCounts = await _placeRepo.AllAsNoTracking()
                .GroupBy(p => p.Activity.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            return new HomeViewModel
            {
                TopPlaces = topPlaces,
                CityNames = cities,
                ActivityNames = activities,
                SportCounts = sportCounts
            };
        }

        public async Task<(IEnumerable<News> news, int totalPages)> GetNewsPagedAsync(int page, int pageSize)
        {
            var query = _newsRepo.AllAsNoTracking();
            var totalNews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalNews / pageSize);

            var newsList = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (newsList, totalPages);
        }

        public async Task CreateNewsAsync(News news, IFormFile? imageFile, string webRootPath)
        {
            if (imageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string folderPath = Path.Combine(webRootPath, "images", "news");

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                news.ImageURL = fileName;
            }

            news.CreatedAt = DateTime.Now;
            await _newsRepo.AddAsync(news);
            await _newsRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            var news = await _newsRepo.All().FirstOrDefaultAsync(n => n.Id == id);
            if (news == null) return false;

            await _newsRepo.DeleteAsync(news);
            await _newsRepo.SaveChangesAsync();
            return true;
        }
    }
}