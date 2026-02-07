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
    public class DashboardController : Controller
    {
        private readonly IOwnerDashboardService _ownerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IOwnerDashboardService ownerService, UserManager<ApplicationUser> userManager)
        {
            _ownerService = ownerService;
            _userManager = userManager;
        }

        //--- OWNER DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var cities = await _ownerService.GetCitiesAsync();
            var activities = await _ownerService.GetActivitiesAsync();

            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            ViewBag.ActivityTypes = new SelectList(activities, "Id", "Name");

            var model = await _ownerService.GetOwnerStatsAsync(userId);
            return View(model);
        }
    }
}