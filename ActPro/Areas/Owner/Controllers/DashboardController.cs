using ActPro.DAL;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

            var model = await ownerService.GetOwnerStatsAsync(userId);

            return View(model);
        }
    }
}