using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        //---Toggle Admin Role---//
        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                TempData["Success"] = $"Администраторски права на {user.FirstName} {user.LastName} бяха премахнати.";
                await _auditService.LogAsync("Edit User", "User", userId, $"Премахнати админ права на: {user.FirstName} {user.LastName} ({user.Email})");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["Success"] = $"{user.FirstName} {user.LastName} вече е администратор.";
                await _auditService.LogAsync("Edit User", "User", userId, $"Дадени админ права на: {user.FirstName} {user.LastName} ({user.Email})");

            }
            return RedirectToAction(nameof(Index));
        }

        //---Toggle Owner Role---//
        [HttpPost]
        public async Task<IActionResult> ToggleOwner(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var isOwner = await _userManager.IsInRoleAsync(user, "Owner");

            if (isOwner)
            {
                await _userManager.RemoveFromRoleAsync(user, "Owner");
                TempData["Success"] = $"Ролята 'Собственик' на {user.FirstName} беше премахната.";
                await _auditService.LogAsync("Edit User", "User", userId, $"Премахнати собственик права на: {user.FirstName} {user.LastName} ({user.Email})");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Owner");
                TempData["Success"] = $"{user.FirstName} вече има роля 'Собственик'.";
                await _auditService.LogAsync("Edit User", "User", userId, $"Дадени собственик права на: {user.FirstName} {user.LastName} ({user.Email})");
            }
            return RedirectToAction(nameof(Index));
        }

        //---Ban User---//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var bannedEntry = new BannedUser
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                BannedAt = DateTime.Now
            };
            _context.BannedUsers.Add(bannedEntry);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Потребителят {user.Email} беше блокиран и изтрит.";
                await _auditService.LogAsync("Ban User", "User", userId, $"Блокиран и изтрит: {user.FirstName} {user.LastName} | Email: {user.Email} | Тел: {user.PhoneNumber}");
            }
            else
            {
                TempData["Error"] = "Възникна грешка при блокиране.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}