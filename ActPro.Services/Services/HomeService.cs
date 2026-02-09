using ActPro.DAL.Data;
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
        private readonly ApplicationDbContext _context;

        public HomeService(IRepository<Place> placeRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo, IRepository<News> newsRepo, ApplicationDbContext context)
        {
            _placeRepo = placeRepo;
            _cityRepo = cityRepo;
            _activityRepo = activityRepo;
            _newsRepo = newsRepo;
            _context = context;
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

        public async Task<(IEnumerable<News> news, int totalPages)> GetNewsPagedAsync(int page, int pageSize, string? userId)
        {
            var query = _newsRepo.AllAsNoTracking();
            var totalNews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalNews / pageSize);

            var newsList = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                var userLikedIds = await _context.NewsLikes
                .Where(l => l.UserId == userId)
                .Select(l => l.NewsId)
                .ToListAsync();

                foreach (var item in newsList)
                {
                    item.IsLikedByCurrentUser = userLikedIds.Contains(item.Id);
                }
            }

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

        public async Task<bool> DeleteNewsAsync(int id, string webRootPath)
        {
            var news = await _newsRepo.All().FirstOrDefaultAsync(n => n.Id == id);
            if (news == null) return false;
            if (!string.IsNullOrEmpty(news.ImageURL) && news.ImageURL != "default.jpg")
            {
                string filePath = Path.Combine(webRootPath, "images", "news", news.ImageURL);

                try
                {
                    if (File.Exists(filePath))
                    {
                         File.Delete(filePath);
                    }
                }
                catch (IOException ex)
                {
                }
            }
            await _newsRepo.DeleteAsync(news);
            await _newsRepo.SaveChangesAsync();
            return true;
        }

        public async Task<(int likes, bool isLiked)> LikeNewsAsync(int newsId, string userId)
        {
            var news = await _newsRepo.All().FirstOrDefaultAsync(n => n.Id == newsId);
            if (news == null) return (0, false);

            var existingLike = await _context.NewsLikes
            .FirstOrDefaultAsync(l => l.NewsId == newsId && l.UserId == userId);

            bool nowLiked;
            if (existingLike != null)
            {
                _context.NewsLikes.Remove(existingLike);
                news.Likes = Math.Max(0, news.Likes - 1);
                nowLiked = false;
            }
            else
            {
                await _context.NewsLikes.AddAsync(new NewsLikes { NewsId = newsId, UserId = userId });
                news.Likes++;
                nowLiked = true;
            }

            await _context.SaveChangesAsync();

            return (news.Likes, nowLiked);
        }
    }
}