using ActPro.Domain;
using ActPro.Services;
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
                    TempData["Success"] = DomainResources.ReservationCancelledSuccess;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, string reservationTime)
        {
            if (!TimeOnly.TryParse(reservationTime, out var newTimeOnly))
            {
                TempData["Error"] = DomainResources.Error;
                return RedirectToAction(nameof(Index));
            }

            var res = await context.Reservations
            .Include(r => r.AspNetUser)
            .Include(r => r.Place)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            string oldTime = res.ReservationTime.ToString();
            string newTime = newTimeOnly.ToString("HH:mm");

            if (await resService.UpdateReservationTimeAsync(id, newTimeOnly))
            {
                await emailSender.SendReservationTimeChangedAsync(
                    res.AspNetUser.Email,
                    res.AspNetUser.FirstName,
                    res.Place.Name,
                    oldTime,
                    newTime,
                    res.ReservationDate.ToString());
                TempData["Success"] = DomainResources.ReservationTimeChangedSuccess;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
