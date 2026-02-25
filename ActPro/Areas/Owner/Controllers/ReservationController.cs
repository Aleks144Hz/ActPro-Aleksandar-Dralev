using ActPro.DAL;
using ActPro.Domain;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class ReservationsController(IReservationDashboardService resService, UserManager<ApplicationUser> userManager, DAL.Data.ApplicationDbContext context, Services.Interfaces.IEmailSender emailSender) : Controller
    {
        //--- DELETE RESERVATION---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await context.Reservations
             .Include(r => r.AspNetUser)
             .Include(r => r.Place)
             .FirstOrDefaultAsync(r => r.Id == id);

            if (res != null)
            {
                if (res.AspNetUserId != null && res.AspNetUser != null)
                {
                    string userEmail = res.AspNetUser.Email;
                    string userFirstName = res.AspNetUser.FirstName;
                    string placeName = res.Place.Name;
                    string formattedDate = res.ReservationDate.ToString();
                    string timeSlot = res.ReservationTime.ToString();
                    await emailSender.SendBookingCancellationAsync(userEmail, userFirstName, placeName, formattedDate, timeSlot);
                }
                if (await resService.DeleteReservationAsync(id))
                {
                    TempData["Success"] = DomainResources.ReservationClosed;
                }
            }

            return RedirectToAction("Index", "Dashboard");
        }

        //--- EDIT TIME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(int id, string reservationTime)
        {
            if (!TimeOnly.TryParse(reservationTime, out var newTimeOnly))
            {
                TempData["Error"] = DomainResources.Error;
                return RedirectToAction("Index", "Dashboard");
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
                TempData["Success"] = DomainResources.ReservationTimeEdited;
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManual(int placeId, string customerNote, DateOnly date, TimeOnly time)
        {
            if (string.IsNullOrEmpty(customerNote))
            {
                TempData["Error"] = DomainResources.Error;
                return RedirectToAction("Index", "Dashboard");
            }

            if (await resService.CreateManualReservationAsync(placeId, customerNote, date, time))
            {
                TempData["Success"] = DomainResources.ReservationBlocked;
            }
            else
            {
                TempData["Error"] = DomainResources.Error;
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}