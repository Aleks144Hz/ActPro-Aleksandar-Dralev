using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;
        public PlacesController(ApplicationDbContext context, IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place)
        {
            var userId = _userManager.GetUserId(User);
            place.OwnerId = userId;
            place.IsApproved = false;

            if (ModelState.IsValid)
            {
                _context.Add(place);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Dashboard");
            }
            return RedirectToAction("Index", "Dashboard");
        }

        // Edit Place
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Place place, IEnumerable<IFormFile> imageFiles)
        {
            var userId = _userManager.GetUserId(User);

            var existingPlace = await _context.Places.AsNoTracking().FirstOrDefaultAsync(p => p.Id == place.Id);
            if (existingPlace == null || existingPlace.OwnerId != userId)
                return Forbid();

            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");
            ModelState.Remove("PlaceImages");
            ModelState.Remove("Rating");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ChangeTracker.Clear();

                    place.OwnerId = userId;
                    place.IsApproved = existingPlace.IsApproved;

                    _context.Update(place);
                    _context.Entry(place).Collection(p => p.PlaceImages).IsModified = false;

                    await _context.SaveChangesAsync();

                    if (imageFiles != null && imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/places", fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }
                                string dbPath = "/images/places/" + fileName;
                                await _context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({place.Id}, {dbPath})");
                            }
                        }
                    }
                    TempData["Success"] = "Успешно редактирахте обекта.";
                    await _auditService.LogAsync("Edit Place", "Place", place.Id.ToString(), $"Променени данни за: {place.Name}");
                    return RedirectToAction("Index", "Dashboard", new { area = "Owner" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Грешка при запис: " + ex.Message);
                }
            }
            return RedirectToAction("Index", "Dashboard", new { area = "Owner" });
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.PlaceImages.FindAsync(imageId);
            if (image == null) return NotFound();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            int placeId = image.PlaceId ?? 0;
            _context.PlaceImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
