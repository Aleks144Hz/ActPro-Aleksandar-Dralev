using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        public ReservationsController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var allReservations = await _context.Reservations
                .Include(r => r.Place)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(allReservations);
        }

        //---Delete Reservation---//
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Резервацията беше анулирана успешно.";
                await _auditService.LogAsync("Delete Reservation", "Reservation", id.ToString(), $"Изтрита резервация на {reservation.FirstName} {reservation.LastName} " + $"за дата {reservation.ReservationDate:dd.MM.yyyy} в {reservation.ReservationTime}");
            }
            return RedirectToAction(nameof(Index));
        }

        //---Edit Reservation Time---//
        [HttpGet]
        public async Task<IActionResult> EditTime(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }
    }
}
