using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController(IAdminDashboardService adminService) : Controller
    {
        //--- ADMIN DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stats = await adminService.GetAdminStatsAsync();

            ViewBag.TotalReservations = stats.TotalReservations;
            ViewBag.TotalUsers = stats.TotalUsers;
            ViewBag.PendingComments = stats.PendingComments;
            ViewBag.TotalPlaces = stats.TotalPlaces;

            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string period = "week")
        {
            var data = await adminService.GetChartDataAsync(period);
            return Json(data);
        }
    }
}