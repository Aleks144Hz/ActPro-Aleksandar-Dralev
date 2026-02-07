using ActPro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly IReservationDashboardService _resService;

        public ReservationsController(IReservationDashboardService resService)
        {
            _resService = resService;
        }

        //--- MENU ---
        public async Task<IActionResult> Index()
        {
            var reservations = await _resService.GetAllReservationsAsync();
            return View(reservations);
        }

        //--- DELETE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _resService.DeleteReservationAsync(id))
                TempData["Success"] = "Резервацията беше анулирана.";

            return RedirectToAction(nameof(Index));
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, TimeOnly reservationTime)
        {
            if (await _resService.UpdateReservationTimeAsync(id, reservationTime))
                TempData["Success"] = "Часът беше променен успешно!";

            return RedirectToAction(nameof(Index));
        }
    }
}
