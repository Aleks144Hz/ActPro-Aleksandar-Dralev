using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ActPro.Controllers
{
    [Authorize]
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public OwnerController(ApplicationDbContext context, IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
        }

        //ADD NEW PLACE
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name");
            return View(new Place());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                try
                {
                    place.OwnerId = _userManager.GetUserId(User);
                    place.IsApproved = false;
                    place.Rating = 0;
                    place.City = null;
                    place.Activity = null;
                    _context.Places.Add(place);
                    await _context.SaveChangesAsync();
                    var currentUser = await _userManager.GetUserAsync(User);

                    if (imageFiles != null && imageFiles.Any())
                    {
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/places");
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
                                await _context.Database.ExecuteSqlInterpolatedAsync(
                                    $"INSERT INTO PlaceImages (PlaceId, ImageUrl) VALUES ({place.Id}, {dbPath})");
                            }
                        }
                    }

                    TempData["Success"] = "Вашата заявка е изпратена успешно!";
                    await _auditService.LogAsync("Create Place", "Place", place.Id.ToString(), $"Създаден обект: {place.Name}");
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "Грешка при запис: " + msg);
                }
            }
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_context.Activities, "Id", "Name", place.ActivityId);
            return View(place);
        }
    }
}