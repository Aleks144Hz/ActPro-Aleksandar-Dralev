using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Owner;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActPro.Controllers
{
    [Authorize]
    public class OwnerController(IPlaceService placeService, UserManager<ApplicationUser> userManager, IWebHostEnvironment env) : Controller
    {
        //---Create Place Request---//
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Cities = new SelectList(await placeService.GetCitiesAsync(), "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(await placeService.GetActivitiesAsync(), "Id", "Name");
            return View(new PlaceEntryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Place place, IEnumerable<IFormFile>? imageFiles)
        {
            new[] { "PlaceImages", "City", "Activity", "Owner", "OwnerId" }.ToList().ForEach(k => ModelState.Remove(k));

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = userManager.GetUserId(User);
                    await placeService.CreatePlaceRequestAsync(place, imageFiles, userId, env.WebRootPath);

                    TempData["Success"] = "Вашата заявка е изпратена успешно!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "Грешка при запис: " + msg);
                }
            }

            ViewBag.Cities = new SelectList(await placeService.GetCitiesAsync(), "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(await placeService.GetActivitiesAsync(), "Id", "Name", place.ActivityId);
            return View(place);
        }
    }
}