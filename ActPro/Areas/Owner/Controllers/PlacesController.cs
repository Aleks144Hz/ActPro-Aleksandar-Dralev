using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class PlacesController(IPlaceDashboardService placeService, UserManager<ApplicationUser> userManager, ApplicationDbContext context) : Controller
    {
        //--- EDIT PLACE---
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = userManager.GetUserId(User);
            var place = await placeService.GetByIdAsync(id.Value);

            if (place == null || place.OwnerId != userId) return Forbid();

            ViewBag.Cities = new SelectList(context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(context.Activities, "Id", "Name", place.ActivityId);

            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            var userId = userManager.GetUserId(User);

            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                if (await placeService.UpdatePlaceAsync(place, imageFiles, userId))
                {
                    TempData["Success"] = DomainResources.PlaceUpdatedSuccessfully;
                    return RedirectToAction("Index", "Dashboard");
                }
                return Forbid();
            }

            ViewBag.Cities = new SelectList(context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(context.Activities, "Id", "Name", place.ActivityId);
            return View(place);
        }

        //--- DELETE IMAGE ---
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            if (await placeService.DeleteImageAsync(imageId))
            {
                return Ok();
            }
            return BadRequest();
        }

        //--- MANAGE SCHEDULE ---
        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int placeId)
        {
            var userId = userManager.GetUserId(User);
            var place = await placeService.GetByIdAsync(placeId);

            if (place == null || place.OwnerId != userId) return Forbid();

            return View(place);
        }

        //--- CLOSE DATE RANGE ---
        [HttpPost]
        public async Task<IActionResult> CloseDateRange(int placeId, DateTime startDate, DateTime endDate, string reason)
        {
            var userId = userManager.GetUserId(User);
            var place = await placeService.GetByIdAsync(placeId);

            if (place == null || place.OwnerId != userId) return Forbid();

            if (await placeService.AddClosuresAsync(placeId, startDate, endDate, reason))
            {
                TempData["Success"] = DomainResources.DateClosedSuccessfully;
            }
            else
            {
                TempData["Error"] = DomainResources.DateError;
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //--- OPEN DATE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDate(int id)
        {
            if (await placeService.RemoveClosureAsync(id))
            {
                TempData["Success"] = DomainResources.DateOpenedSuccessfully;
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}