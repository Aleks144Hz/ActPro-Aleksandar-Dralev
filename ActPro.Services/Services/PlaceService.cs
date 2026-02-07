using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public PlaceService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<City>> GetCitiesAsync() => await _context.Cities.OrderBy(c => c.Name).ToListAsync();

        public async Task<IEnumerable<Activity>> GetActivitiesAsync() => await _context.Activities.OrderBy(a => a.Name).ToListAsync();

        public async Task<bool> CreatePlaceRequestAsync(Place place, IEnumerable<IFormFile>? imageFiles, string userId, string webRootPath)
        {
            place.OwnerId = userId;
            place.IsApproved = false;
            place.Rating = 0;
            place.City = null;
            place.Activity = null;

            _context.Places.Add(place);
            await _context.SaveChangesAsync();

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

                        await _context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({place.Id}, {dbPath})");
                    }
                }
            }

            await _auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"Създаден обект: {place.Name}");
            return true;
        }
    }
}