using ActPro.DAL;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class DashboardController(IOwnerDashboardService ownerService, UserManager<ApplicationUser> userManager) : Controller
    {
        //--- OWNER DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var cities = await ownerService.GetCitiesAsync();
            var activities = await ownerService.GetActivitiesAsync();

            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(activities, "Id", "Name");

            var model = await ownerService.GetOwnerStatsAsync(userId);
            return View(model);
        }
    }
}