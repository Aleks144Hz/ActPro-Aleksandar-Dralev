using ActPro.DAL;
using ActPro.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalReservations = await _context.Reservations.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.PendingComments = await _context.Comments.CountAsync();

            var latestReservations = await _context.Reservations
                .Include(r => r.Place)
                .Include(r => r.AspNetUser)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(latestReservations);
        }
    }
}
