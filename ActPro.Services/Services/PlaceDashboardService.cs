using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class PlaceDashboardService(ApplicationDbContext context, IAuditService auditService, UserManager<ApplicationUser> userManager) : IPlaceDashboardService
    {
        public async Task<IEnumerable<Place>> GetAllPlacesAsync()
        {
            return await context.Places
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .ToListAsync();
        }

        public async Task<IEnumerable<Place>> GetOwnerPlacesAsync(string ownerId)
        {
            return await context.Places
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
        }

        public async Task<Place?> GetByIdAsync(int id)
        {
            return await context.Places
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .Include(p => p.Reservations)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        //--- CREATE ---
        public async Task<bool> CreatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string userId, bool isApproved)
        {
            place.OwnerId = userId;
            place.Rating = 0;
            place.IsApproved = isApproved;

            context.Places.Add(place);
            await context.SaveChangesAsync();

            if (images != null && images.Any())
            {
                await ProcessImagesRawSql(place.Id, images);
            }

            if (place.OwnerId != null)
            {
                var user = await userManager.FindByIdAsync(place.OwnerId);
                if (user != null && !await userManager.IsInRoleAsync(user, "Owner"))
                {
                    await userManager.AddToRoleAsync(user, "Owner");
                }
            }

            await auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"Създаден обект: {place.Name}");
            return true;
        }

        //--- EDIT ---
        public async Task<bool> UpdatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string? ownerId = null)
        {
            if (ownerId != null)
            {
                var existing = await context.Places.AsNoTracking().FirstOrDefaultAsync(p => p.Id == place.Id);
                if (existing == null || existing.OwnerId != ownerId) return false;
            }

            place.IsApproved = true;

            context.ChangeTracker.Clear();
            context.Update(place);
            context.Entry(place).Collection(p => p.PlaceImages).IsModified = false;
            await context.SaveChangesAsync();

            if (images != null && images.Any())
            {
                await ProcessImagesRawSql(place.Id, images);
            }

            await auditService.LogAsync("Edit Place", "Place", place.Id.ToString(), $"Променени данни за: {place.Name}");
            return true;
        }

        //--- DELETE ---
        public async Task<bool> DeletePlaceAsync(int id)
        {
            var place = await context.Places.Include(p => p.PlaceImages).FirstOrDefaultAsync(p => p.Id == id);
            if (place == null) return false;

            if (place.PlaceImages != null && place.PlaceImages.Any())
            {
                foreach (var img in place.PlaceImages)
                {
                    // Използваме Path.Combine за сигурност
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));

                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    catch (IOException)
                    {
                        // Логнете грешката ако е необходимо, но продължаваме нататък
                    }
                }
            }

            context.PlaceImages.RemoveRange(place.PlaceImages);
            context.Comments.RemoveRange(context.Comments.Where(c => c.PlaceId == id));
            context.Reservations.RemoveRange(context.Reservations.Where(r => r.PlaceId == id));

            context.Places.Remove(place);
            await context.SaveChangesAsync();

            await auditService.LogAsync("Delete Place", "Place", id.ToString(), $"Изтрит обект: {place.Name}");
            return true;
        }

        //--- APPROVE ---
        public async Task<bool> ApprovePlaceAsync(int id)
        {
            var place = await context.Places.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
            if (place == null) return false;

            place.IsApproved = true;
            if (place.OwnerId != null)
            {
                var user = await userManager.FindByIdAsync(place.OwnerId);
                if (user != null && !await userManager.IsInRoleAsync(user, "Owner"))
                {
                    await userManager.AddToRoleAsync(user, "Owner");
                }
            }

            await context.SaveChangesAsync();
            await auditService.LogAsync("Approve Place", "Place", id.ToString(), $"Одобрен обект: {place.Name}");
            return true;
        }

        //--- IMAGE UPLOAD WITH RAW SQL ---
        private async Task ProcessImagesRawSql(int placeId, IEnumerable<IFormFile> images)
        {
            foreach (var file in images.Where(f => f.Length > 0))
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/places", fileName);

                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string dbPath = "/images/places/" + fileName;
                await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({placeId}, {dbPath})");
            }
        }

        //--- DELETE IMAGE ---
        public async Task<bool> DeleteImageAsync(int imageId)
        {
            var image = await context.PlaceImages.FindAsync(imageId);
            if (image == null) return false;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (File.Exists(filePath)) File.Delete(filePath);

            context.PlaceImages.Remove(image);
            await context.SaveChangesAsync();
            return true;
        }

        //--- CLOSURES ---
        public async Task<bool> AddClosuresAsync(int placeId, DateTime start, DateTime end, string reason)
        {
            if (end < start) return false;
            var closuresToAdd = new List<PlaceClosure>();
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (!await context.PlaceClosures.AnyAsync(c => c.PlaceId == placeId && c.ClosureDate.Date == date))
                {
                    closuresToAdd.Add(new PlaceClosure { PlaceId = placeId, ClosureDate = date, Reason = reason });
                }
            }
            if (closuresToAdd.Any())
            {
                context.PlaceClosures.AddRange(closuresToAdd);
                await context.SaveChangesAsync();
                await auditService.LogAsync("Add Closure", "Place", placeId.ToString(), $"Заключен период от {start:dd.MM.yyyy} до {end:dd.MM.yyyy}. Причина: {reason}");
            }
            return true;
        }

        public async Task<bool> RemoveClosureAsync(int closureId)
        {
            var closure = await context.PlaceClosures.FindAsync(closureId);
            if (closure == null) return false;
            int placeId = closure.PlaceId;
            DateTime closedDate = closure.ClosureDate;
            context.PlaceClosures.Remove(closure);
            await context.SaveChangesAsync();
            await auditService.LogAsync("Remove Closure", "Place", placeId.ToString(), $"Отключена дата {closedDate:dd.MM.yyyy} за обект ID: {closureId}");
            return true;
        }
    }
}