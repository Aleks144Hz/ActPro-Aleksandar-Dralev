using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain;
using ActPro.Domain.Models.Areas;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

            var cities = await context.Cities.ToListAsync();
            var activities = await context.Activities.ToListAsync();

            var model = new PlaceFormViewModel
            {
                Id = place.Id,
                Name = place.Name ?? "",
                NameEn = place.NameEn,
                Description = place.Description,
                DescriptionEn = place.DescriptionEn,
                Address = place.Address,
                Price = place.Price ?? 0,
                Capacity = place.Capacity ?? 0,
                IsOutdoor = place.IsOutdoor,
                CityId = place.CityId ?? 0,
                ActivityId = place.ActivityId ?? 0,
                Rating = place.Rating ?? 0,
                OwnerId = place.OwnerId ?? "",
                CityOptions = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                ActivityOptions = activities.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList(),
                ExistingImages = place.PlaceImages?.Select(img => new PlaceImageViewModel { Id = img.Id, Url = img.ImageUrl }).ToList() ?? new()
            };

            ViewBag.Cities = new SelectList(cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(activities, "Id", "Name", place.ActivityId);

            return View(model);
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