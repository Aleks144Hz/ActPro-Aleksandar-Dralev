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
        //---Home Page---
        public async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            var topPlaces = await placeRepo.AllAsNoTracking()
            .Include(p => p.City)
            .Include(p => p.PlaceImages)
            .OrderByDescending(p => p.Rating)
            .Take(3)
            .ToListAsync();

            var citiesAll = await cityRepo.AllAsNoTracking().ToListAsync();
            var activitiesAll = await activityRepo.AllAsNoTracking().ToListAsync();
            var isEnglish = System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en";
            
            var cities = citiesAll
                .Select(c => isEnglish && !string.IsNullOrEmpty(c.NameEn) ? c.NameEn : c.Name)
                .Distinct()
                .ToList();

            var activities = activitiesAll
                .Select(a => isEnglish && !string.IsNullOrEmpty(a.NameEn) ? a.NameEn : a.Name)
                .Distinct()
                .ToList();

            var sportCountsList = await placeRepo.AllAsNoTracking()
            .Include(p => p.Activity)
            .Where(p => p.Activity != null)
            .GroupBy(p => p.Activity)
            .Select(g => new { 
                Activity = g.Key, 
                Count = g.Count(),
                Name = g.Key.Name,
                NameEn = g.Key.NameEn
            })
            .ToListAsync();

            var sportCounts = new Dictionary<string, int>();
            foreach (var item in sportCountsList)
            {
                if (!string.IsNullOrEmpty(item.Name))
                    sportCounts[item.Name] = item.Count;
                if (!string.IsNullOrEmpty(item.NameEn))
                    sportCounts[item.NameEn] = item.Count;
            }

            return new HomeViewModel
            {
                TopPlaces = topPlaces,
                CityNames = cities,
                ActivityNames = activities,
                SportCounts = sportCounts
            };
        }

        //---News Page---
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

        //---Admin News Management---
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

        //---News Like/Unlike---
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