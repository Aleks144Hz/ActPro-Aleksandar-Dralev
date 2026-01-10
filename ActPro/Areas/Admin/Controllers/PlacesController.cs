using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        public PlacesController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        public async Task<IActionResult> Index()
        {
            var places = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Activity)
                .Include(p => p.PlaceImages)
                .Include(p => p.PlaceClosures)
                .ToListAsync();
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name");

            return View(places);
        }
        
        //--- CREATE PLACE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");

            place.Rating = 0;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Places.Add(place);
                    await _context.SaveChangesAsync();
                    if (imageFiles != null && imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0)
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
                                await _context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({place.Id}, {dbPath})");
                            }
                        }
                    }
                    TempData["Success"] = "Обектът беше създаден успешно.";
                    await _auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"Създаден обект: {place.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Грешка при запис: " + ex.Message);
                }
            }
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name", place.ActivityId);
            return RedirectToAction(nameof(Index));
        }

        //--- DELETE PLACE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }
            var images = _context.PlaceImages.Where(img => img.PlaceId == id).ToList();
            foreach (var img in images)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            _context.PlaceImages.RemoveRange(images);
            var relatedComments = _context.Comments.Where(c => c.PlaceId == id);
            _context.Comments.RemoveRange(relatedComments);
            var relatedReservations = _context.Reservations.Where(r => r.PlaceId == id);
            _context.Reservations.RemoveRange(relatedReservations);
            _context.Places.Remove(place);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Обектът '{place.Name}' и всички свързани данни бяха изтрити успешно.";
            await _auditService.LogAsync("Delete Place", "Place", id.ToString(), $"Изтрит обект: {place.Name}");
            return RedirectToAction(nameof(Index));
        }

        //--- EDIT PLACE ---
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
        public async Task<IActionResult> Edit(int id, Place place, IEnumerable<IFormFile> imageFiles)
        {
            if (id != place.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ChangeTracker.Clear();
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
                                await _context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({id}, {dbPath})");
                            }
                        }
                    }
                    TempData["Success"] = "Успешно редактирахте данните на обекта.";
                    await _auditService.LogAsync("Edit Place", "Place", place.Id.ToString(), $"Променени данни за: {place.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Грешка при запис: " + ex.Message);
                }
            }
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name", place.ActivityId);
            return View(place);
        }
        private bool PlaceExists(int id)
        {
            return _context.Places.Any(e => e.Id == id);
        }

        //--- DELETE IMAGE ---
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

        //--- MANAGE PLACE SCHEDULE ---
        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int placeId)
        {
            var place = await _context.Places.Include(p => p.PlaceClosures).Include(p => p.Reservations).FirstOrDefaultAsync(p => p.Id == placeId);
            if (place == null) return NotFound();
            return View(place);
        }

        [HttpPost]
        public async Task<IActionResult> CloseDate(int placeId, DateTime dateToClose, string reason)
        {
            var exists = await _context.PlaceClosures.AnyAsync(c => c.PlaceId == placeId && c.ClosureDate.Date == dateToClose.Date);

            if (!exists)
            {
                _context.PlaceClosures.Add(new PlaceClosure
                {
                    PlaceId = placeId,
                    ClosureDate = dateToClose,
                    Reason = reason
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Датата беше заключена успешно.";
                await _auditService.LogAsync("Add Closure", "Place", placeId.ToString(), $"Заключена дата {dateToClose:dd.MM.yyyy} за обект ID: {placeId}. Причина: {reason}");
            }
            return RedirectToAction(nameof(Index));
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

                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
