using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain;
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
        public async Task<IActionResult> Index(PlaceEntryViewModel model, IEnumerable<IFormFile>? imageFiles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = new SelectList(await placeService.GetCitiesAsync(), "Id", "Name", model.CityId);
                ViewBag.ActivityTypes = new SelectList(await placeService.GetActivitiesAsync(), "Id", "Name", model.ActivityId);
                return View(model);
            }

            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();
                if (!user.EmailConfirmed)
                {
                    TempData["Error"] = DomainResources.ProfileNotApprovedForCreatingPlace;
                    return RedirectToAction("Index", "Home");
                }

                var place = new Place
                {
                    Name = model.Name,
                    Address = model.Address,
                    Description = model.Description,
                    Price = model.Price,
                    Capacity = model.Capacity,
                    IsOutdoor = model.IsOutdoor,
                    CityId = model.CityId,
                    ActivityId = model.ActivityId
                };

                var userId = userManager.GetUserId(User);
                await placeService.CreatePlaceRequestAsync(place, imageFiles, userId, env.WebRootPath);

                TempData["Success"] = DomainResources.RequestSentSuccessfully;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", DomainResources.ErrorWhileTryingToFill + msg);
            }

            ViewBag.Cities = new SelectList(await placeService.GetCitiesAsync(), "Id", "Name", model.CityId);
            ViewBag.ActivityTypes = new SelectList(await placeService.GetActivitiesAsync(), "Id", "Name", model.ActivityId);
            return View(model);
        }
    }
}