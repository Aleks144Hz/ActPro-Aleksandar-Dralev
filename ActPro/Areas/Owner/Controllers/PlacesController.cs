using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class PlacesController : Controller
    {
        private readonly IPlaceDashboardService _placeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public PlacesController(IPlaceDashboardService placeService, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _placeService = placeService;
            _userManager = userManager;
            _db = db;
        }

        //--- EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var place = await _placeService.GetByIdAsync(id.Value);

            if (place == null || place.OwnerId != userId) return Forbid();

            ViewBag.Cities = new SelectList(_db.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_db.Activities, "Id", "Name", place.ActivityId);

            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            var userId = _userManager.GetUserId(User);

            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                if (await _placeService.UpdatePlaceAsync(place, imageFiles, userId))
                {
                    TempData["Success"] = "Успешно редактирахте данните на обекта.";
                    return RedirectToAction("Index", "Dashboard");
                }
                return Forbid();
            }

            ViewBag.Cities = new SelectList(_db.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(_db.Activities, "Id", "Name", place.ActivityId);
            return View(place);
        }

        //--- DELETE IMAGE ---
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            if (await _placeService.DeleteImageAsync(imageId))
            {
                return Ok();
            }
            return BadRequest();
        }

        //--- MANAGE SCHEDULE ---
        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int placeId)
        {
            var userId = _userManager.GetUserId(User);
            var place = await _placeService.GetByIdAsync(placeId);

            if (place == null || place.OwnerId != userId) return Forbid();

            return View(place);
        }

        //--- CLOSE DATE RANGE ---
        [HttpPost]
        public async Task<IActionResult> CloseDateRange(int placeId, DateTime startDate, DateTime endDate, string reason)
        {
            var userId = _userManager.GetUserId(User);
            var place = await _placeService.GetByIdAsync(placeId);

            if (place == null || place.OwnerId != userId) return Forbid();

            if (await _placeService.AddClosuresAsync(placeId, startDate, endDate, reason))
            {
                TempData["Success"] = $"Успешно затворихте избрания период.";
            }
            else
            {
                TempData["Error"] = "Крайната дата не може да бъде преди началната.";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //--- OPEN DATE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDate(int id)
        {
            if (await _placeService.RemoveClosureAsync(id))
            {
                TempData["Success"] = "Датата беше отключена успешно.";
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}