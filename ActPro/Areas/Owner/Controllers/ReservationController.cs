using ActPro.DAL;
using ActPro.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> EditTime(int id, DateOnly reservationDate, TimeOnly reservationTime)
        {
            var userId = _userManager.GetUserId(User);
            var res = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null || res.Place.OwnerId != userId) return Forbid();

            res.ReservationDate = reservationDate;
            res.ReservationTime = reservationTime;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var res = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res != null && res.Place.OwnerId == userId)
            {
                _context.Reservations.Remove(res);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
