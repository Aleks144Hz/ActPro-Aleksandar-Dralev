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
    public class OwnerController : Controller
    {
        private readonly IPlaceService _placeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public OwnerController(IPlaceService placeService, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _placeService = placeService;
            _userManager = userManager;
            _env = env;
        }

        //---Create Place Request---//
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Cities = new SelectList(await _placeService.GetCitiesAsync(), "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(await _placeService.GetActivitiesAsync(), "Id", "Name");
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
                    var userId = _userManager.GetUserId(User);
                    await _placeService.CreatePlaceRequestAsync(place, imageFiles, userId, _env.WebRootPath);

                    TempData["Success"] = "Вашата заявка е изпратена успешно!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "Грешка при запис: " + msg);
                }
            }

            ViewBag.Cities = new SelectList(await _placeService.GetCitiesAsync(), "Id", "Name", place.CityId);
            ViewBag.ActivityTypes = new SelectList(await _placeService.GetActivitiesAsync(), "Id", "Name", place.ActivityId);
            return View(place);
        }
    }
}