using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
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
        private readonly IAuditService _auditService;
        public ReservationsController(ApplicationDbContext context, IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
        }
        //---Delete Reservation---//
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

        //---Edit Reservation Time---//
        [HttpGet]
        public async Task<IActionResult> EditTime(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTime(Reservation model)
        {
            var res = await _context.Reservations.FindAsync(model.Id);
            if (res == null) return NotFound();

            var oldTime = res.ReservationTime;
            var newTime = model.ReservationTime;

            res.ReservationTime = model.ReservationTime;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът на резервацията беше променен успешно!";
            await _auditService.LogAsync("Edit Reservation", "Reservation", res.Id.ToString(), $"Променен час на резервация за {res.FirstName} {res.LastName}. " + $"Стар час: {oldTime} -> Нов час: {newTime}");
            return RedirectToAction("Index", "Dashboard");
        }   
    }
}
