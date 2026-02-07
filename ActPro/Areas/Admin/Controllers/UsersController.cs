using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            if (await _userService.ToggleRoleAsync(userId, "Admin"))
            {
                TempData["Success"] = "Администраторските права бяха променени.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOwner(string userId)
        {
            if (await _userService.ToggleRoleAsync(userId, "Owner"))
            {
                TempData["Success"] = "Собственик правата бяха променени.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            if (await _userService.BanUserAsync(userId))
            {
                TempData["Success"] = "Потребителят беше блокиран и изтрит успешно.";
            }
            else
            {
                TempData["Error"] = "Възникна грешка при блокирането.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}