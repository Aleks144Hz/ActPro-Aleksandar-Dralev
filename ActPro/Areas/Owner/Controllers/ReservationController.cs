using ActPro.DAL;
using ActPro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class ReservationsController : Controller
    {
        private readonly IReservationDashboardService _resService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(IReservationDashboardService resService, UserManager<ApplicationUser> userManager)
        {
            _resService = resService;
            _userManager = userManager;
        }

        //--- DELETE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (await _resService.DeleteReservationAsync(id, userId))
                TempData["Success"] = "Резервацията беше анулирана.";

            return RedirectToAction("Index", "Dashboard");
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, TimeOnly reservationTime)
        {
            var userId = _userManager.GetUserId(User);
            if (await _resService.UpdateReservationTimeAsync(id, reservationTime, userId))
                TempData["Success"] = "Часът беше променен!";

            return RedirectToAction("Index", "Dashboard");
        }
    }
}