using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class PlaceService(ApplicationDbContext context, IAuditService auditService) : IPlaceService
    {
        // --- Places ---
        public async Task<IEnumerable<City>> GetCitiesAsync() => await context.Cities.OrderBy(c => c.Name).ToListAsync();

        public async Task<IEnumerable<Activity>> GetActivitiesAsync() => await context.Activities.OrderBy(a => a.Name).ToListAsync();

        public async Task<bool> CreatePlaceRequestAsync(Place place, IEnumerable<IFormFile>? imageFiles, string userId, string webRootPath)
        {
            place.OwnerId = userId;
            place.IsApproved = false;
            place.Rating = 0;
            place.City = null;
            place.Activity = null;

            context.Places.Add(place);
            await context.SaveChangesAsync();

            if (imageFiles != null && imageFiles.Any())
            {
                var uploadPath = Path.Combine(webRootPath, "images/places");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        string dbPath = "/images/places/" + fileName;

                        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({place.Id}, {dbPath})");
                    }
                }
            }

            await auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"Създаден обект: {place.Name}");
            return true;
        }
    }
}