using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain.Repository;
using ActPro.Models;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services
{
    public class HomeService(IRepository<Place> placeRepo, IRepository<City> cityRepo, IRepository<Activity> activityRepo, IRepository<News> newsRepo, ApplicationDbContext context) : IHomeService
    {
        public async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            var topPlaces = await placeRepo.AllAsNoTracking()
            .Include(p => p.City)
            .Include(p => p.PlaceImages)
            .OrderByDescending(p => p.Rating)
            .Take(3)
            .ToListAsync();

            var cities = await cityRepo.AllAsNoTracking()
            .Select(c => c.Name)
            .Distinct()
            .ToListAsync();

            var activities = await activityRepo.AllAsNoTracking()
            .Select(a => a.Name)
            .Distinct()
            .ToListAsync();

            var sportCounts = await placeRepo.AllAsNoTracking()
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
            var query = newsRepo.AllAsNoTracking();
            var totalNews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalNews / pageSize);

            var newsList = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                var userLikedIds = await context.NewsLikes
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

        public async Task CreateNewsAsync(string title, string content, IFormFile? imageFile, string webRootPath)
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
                var news = new News
                {
                    Title = title,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    ImageURL = fileName
                };
                await newsRepo.AddAsync(news);
                await newsRepo.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteNewsAsync(int id, string webRootPath)
        {
            var news = await newsRepo.All().FirstOrDefaultAsync(n => n.Id == id);
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
            await newsRepo.DeleteAsync(news);
            await newsRepo.SaveChangesAsync();
            return true;
        }

        public async Task<(int likes, bool isLiked)> LikeNewsAsync(int newsId, string userId)
        {
            var news = await newsRepo.All().FirstOrDefaultAsync(n => n.Id == newsId);
            if (news == null) return (0, false);

            var existingLike = await context.NewsLikes
            .FirstOrDefaultAsync(l => l.NewsId == newsId && l.UserId == userId);

            bool nowLiked;
            if (existingLike != null)
            {
                context.NewsLikes.Remove(existingLike);
                news.Likes = Math.Max(0, news.Likes - 1);
                nowLiked = false;
            }
            else
            {
                await context.NewsLikes.AddAsync(new NewsLikes { NewsId = newsId, UserId = userId });
                news.Likes++;
                nowLiked = true;
            }

            await context.SaveChangesAsync();

            return (news.Likes, nowLiked);
        }
    }
}