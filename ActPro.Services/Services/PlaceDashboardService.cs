using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain;
using ActPro.Domain.Models.Areas;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class PlaceDashboardService(ApplicationDbContext context, IAuditService auditService, UserManager<ApplicationUser> userManager) : IPlaceDashboardService
    {
        //--- INDEX ---
        public async Task<PlacesIndexViewModel> GetPlacesDashboardModelAsync(string? ownerId = null)
        {
            var query = context.Places
            .Include(p => p.City)
            .Include(p => p.Activity)
            .Include(p => p.PlaceImages)
            .Include(p => p.PlaceClosures)
            .AsQueryable();

            if (!string.IsNullOrEmpty(ownerId))
            {
                query = query.Where(p => p.OwnerId == ownerId);
            }

            var places = await query.ToListAsync();

            var cities = await context.Cities
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

            var activities = await context.Activities
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            .ToListAsync();

            return new PlacesIndexViewModel
            {
                Places = places.Select(p => new PlaceViewModel
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    CityName = p.City?.Name ?? DomainResources.NoCities,
                    ActivityName = p.Activity?.Name ?? DomainResources.NoActivities,
                    Price = p.Price ?? 0,
                    IsApproved = p.IsApproved
                }).ToList(),

                CityOptions = cities,
                ActivityOptions = activities,

                EditPlaces = places.ToDictionary(p => p.Id, p => new PlaceFormViewModel
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    Address = p.Address,
                    Description = p.Description,
                    Price = p.Price ?? 0,
                    Capacity = p.Capacity.GetValueOrDefault(),
                    IsOutdoor = p.IsOutdoor ?? false,
                    CityId = p.CityId.GetValueOrDefault(),
                    ActivityId = p.ActivityId.GetValueOrDefault(),
                    Rating = p.Rating ?? 0,
                    OwnerId = p.OwnerId ?? string.Empty,
                    CityOptions = cities,
                    ActivityOptions = activities,
                    ExistingImages = p.PlaceImages?.Select(img => new PlaceImageViewModel
                    {
                        Id = img.Id,
                        Url = img.ImageUrl
                    }).ToList() ?? new()
                }),

                PlaceSchedules = places.ToDictionary(p => p.Id, p => new PlaceScheduleViewModel
                {
                    PlaceId = p.Id,
                    PlaceName = p.Name,
                    Closures = p.PlaceClosures?.Select(c => new ClosureViewModel
                    {
                        Id = c.Id,
                        StartDate = c.ClosureDate,
                        EndDate = c.ClosureDate,
                        Reason = c.Reason
                    }).ToList() ?? new()
                })
            };
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

            await auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"{DomainResources.CreatedPlace}: {place.Name}");
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

            await auditService.LogAsync("Edit Place", "Place", place.Id.ToString(), $"{DomainResources.UpdatedPlace}: {place.Name}");
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
                    }
                }
            }

            context.PlaceImages.RemoveRange(place.PlaceImages);
            context.Comments.RemoveRange(context.Comments.Where(c => c.PlaceId == id));
            context.Reservations.RemoveRange(context.Reservations.Where(r => r.PlaceId == id));

            context.Places.Remove(place);
            await context.SaveChangesAsync();

            await auditService.LogAsync("Delete Place", "Place", id.ToString(), $"{DomainResources.DeletedPlace}: {place.Name}");
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
            await auditService.LogAsync("Approve Place", "Place", id.ToString(), $"{DomainResources.ApprovedPlace}: {place.Name}");
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
                await auditService.LogAsync("Add Closure", "Place", placeId.ToString(), $"{DomainResources.PlaceClosed} {start:dd.MM.yyyy} {DomainResources.To} {end:dd.MM.yyyy}. {DomainResources.Reason}: {reason}");
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
            await auditService.LogAsync("Remove Closure", "Place", placeId.ToString(), $"{DomainResources.PlaceOpened} {closedDate:dd.MM.yyyy} {DomainResources.PlaceId}: {closureId}");
            return true;
        }
    }
}