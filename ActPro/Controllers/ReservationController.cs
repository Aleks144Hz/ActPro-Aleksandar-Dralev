using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int id)
        {
            var place = await _context.Places
                .Include(p => p.PlaceImages)
                .Include(p => p.City)
                .Include(p => p.Activity)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (place == null)
            {
                return NotFound();
            }

            return View("Reservation", place);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int placeId, DateTime date, string timeSlot)
        {
            return RedirectToAction("Confirmation", new { id = placeId });
        }
        public IActionResult Confirmation(int id)
        {
            ViewBag.PlaceId = id;
            return View();
        }
    }
}