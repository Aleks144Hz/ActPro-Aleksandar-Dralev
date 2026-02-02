using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Edit Place
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var place = await _context.Places
            .Include(p => p.PlaceImages)
            .FirstOrDefaultAsync(m => m.Id == id);
            if (place == null) return NotFound();

            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name", place.ActivityId);

            return View(place);
        }
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

        //---Manage Place Schedule---//
        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int placeId)
        {
            var place = await _context.Places.Include(p => p.PlaceClosures).Include(p => p.Reservations).FirstOrDefaultAsync(p => p.Id == placeId);
            if (place == null) return NotFound();
            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseDateRange(int placeId, DateTime startDate, DateTime endDate, string reason)
        {
            if (endDate < startDate)
            {
                TempData["Error"] = "Крайната дата не може да бъде преди началната.";
                return RedirectToAction("Index", "Dashboard");
            }

            var closuresToAdd = new List<PlaceClosure>();

            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                bool exists = await _context.PlaceClosures
                    .AnyAsync(c => c.PlaceId == placeId && c.ClosureDate.Date == date);

                if (!exists)
                {
                    closuresToAdd.Add(new PlaceClosure
                    {
                        PlaceId = placeId,
                        ClosureDate = date,
                        Reason = reason
                    });
                }
            }

            if (closuresToAdd.Any())
            {
                _context.PlaceClosures.AddRange(closuresToAdd);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Успешно затворихте {closuresToAdd.Count} дни.";
                await _auditService.LogAsync("Add Closure", "Place", placeId.ToString(), $"Заключен период от {startDate:dd.MM.yyyy} до {endDate:dd.MM.yyyy}. Причина: {reason}");
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDate(int id)
        {
            var closure = await _context.PlaceClosures.FindAsync(id);
            if (closure != null)
            {
                int placeId = closure.PlaceId;
                DateTime closedDate = closure.ClosureDate;

                _context.PlaceClosures.Remove(closure);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Датата беше отключена успешно.";

                await _auditService.LogAsync("Remove Closure", "Place", placeId.ToString(), $"Отключена дата {closedDate:dd.MM.yyyy} за обект ID: {placeId}");

                return RedirectToAction("Index", "Dashboard");
            }
            return NotFound();
        }
    }
}
