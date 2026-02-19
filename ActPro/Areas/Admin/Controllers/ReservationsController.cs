using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController(IReservationDashboardService resService, DAL.Data.ApplicationDbContext context, Services.Interfaces.IEmailSender emailSender) : Controller
    {
        //--- MENU ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await resService.GetReservationsIndexModelAsync();
            return View(viewModel);
        }

        //--- DELETE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await context.Reservations
            .Include(r => r.AspNetUser)
            .Include(r => r.Place)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (res != null)
            {
                string userEmail = res.AspNetUser.Email;
                string userFirstName = res.AspNetUser.FirstName;
                string placeName = res.Place.Name;
                string formattedDate = res.ReservationDate.ToString();
                string timeSlot = res.ReservationTime.ToString();

                if (await resService.DeleteReservationAsync(id))
                {
                    await emailSender.SendBookingCancellationAsync(userEmail, userFirstName, placeName, formattedDate, timeSlot);
                    TempData["Success"] = "Резервацията беше анулирана";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, TimeOnly reservationTime)
        {
            var res = await context.Reservations
            .Include(r => r.AspNetUser)
            .Include(r => r.Place)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            string oldTime = res.ReservationTime.ToString();
            string newTime = reservationTime.ToString("HH:mm");

            if (await resService.UpdateReservationTimeAsync(id, reservationTime))
            {
                await emailSender.SendReservationTimeChangedAsync(
                    res.AspNetUser.Email,
                    res.AspNetUser.FirstName,
                    res.Place.Name,
                    oldTime,
                    newTime,
                    res.ReservationDate.ToString());
                TempData["Success"] = "Часът беше променен успешно!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
