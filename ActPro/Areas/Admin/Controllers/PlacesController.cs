using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlacesController(IPlaceDashboardService placeService, ApplicationDbContext context) : Controller
    {
        //--- INDEX ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await placeService.GetPlacesDashboardModelAsync();
            return View(viewModel);
        }

        //--- CREATE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                await placeService.CreatePlaceAsync(place, imageFiles, userId, true);
                TempData["Success"] = "Обектът беше създаден успешно.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        //--- EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var place = await placeService.GetByIdAsync(id.Value);
            if (place == null) return NotFound();

            ViewBag.Cities = new SelectList(context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(context.Activities, "Id", "Name", place.ActivityId);

            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Place place, IEnumerable<IFormFile>? imageFiles)
        {
            if (id != place.Id) return NotFound();

            ModelState.Remove("PlaceImages");
            ModelState.Remove("City");
            ModelState.Remove("Activity");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                if (await placeService.UpdatePlaceAsync(place, imageFiles))
                {
                    TempData["Success"] = "Успешно редактирахте данните на обекта.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Cities = new SelectList(context.Cities, "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(context.Activities, "Id", "Name", place.ActivityId);
            return View(place);
        }

        //--- DELETE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await placeService.DeletePlaceAsync(id);
            TempData["Success"] = "Обектът и всички свързани данни бяха изтрити.";
            return RedirectToAction(nameof(Index));
        }

        //--- APPROVE ---
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (await placeService.ApprovePlaceAsync(id))
            {
                TempData["Success"] = "Обектът беше одобрен успешно.";
            }
            return RedirectToAction(nameof(Index));
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

        //--- SCHEDULE MANAGEMENT ---
        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int placeId)
        {
            var place = await placeService.GetByIdAsync(placeId);
            if (place == null) return NotFound();
            return View(place);
        }

        //--- CLOSE DATE RANGE ---
        [HttpPost]
        public async Task<IActionResult> CloseDateRange(int placeId, DateTime startDate, DateTime endDate, string reason)
        {
            if (await placeService.AddClosuresAsync(placeId, startDate, endDate, reason))
            {
                TempData["Success"] = "Датите бяха затворени успешно.";
            }
            else
            {
                TempData["Error"] = "Грешка при затваряне на датите.";
            }
            return RedirectToAction(nameof(Index));
        }

        //--- OPEN DATE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDate(int id)
        {
            if (await placeService.RemoveClosureAsync(id))
            {
                TempData["Success"] = "Датата беше отключена успешно.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}