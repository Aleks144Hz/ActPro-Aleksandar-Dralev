using ActPro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController(IReservationDashboardService resService) : Controller
    {
        //--- MENU ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reservations = await resService.GetAllReservationsAsync();
            return View(reservations);
        }

        //--- DELETE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (await resService.DeleteReservationAsync(id))
                TempData["Success"] = "Резервацията беше анулирана.";

            return RedirectToAction(nameof(Index));
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, TimeOnly reservationTime)
        {
            if (await resService.UpdateReservationTimeAsync(id, reservationTime))
                TempData["Success"] = "Часът беше променен успешно!";

            return RedirectToAction(nameof(Index));
        }
    }
}
