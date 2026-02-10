using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserService userService) : Controller
    {
        //--- Users Management Dashboard ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await userService.GetAllUsersAsync();
            return View(users);
        }

        //--- Toggle Admin Role ---
        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            if (await userService.ToggleRoleAsync(userId, "Admin"))
            {
                TempData["Success"] = "Администраторските права бяха променени.";
            }
            return RedirectToAction(nameof(Index));
        }

        //--- Toggle Owner Role ---
        [HttpPost]
        public async Task<IActionResult> ToggleOwner(string userId)
        {
            if (await userService.ToggleRoleAsync(userId, "Owner"))
            {
                TempData["Success"] = "Собственик правата бяха променени.";
            }
            return RedirectToAction(nameof(Index));
        }

        //--- Ban User ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            if (await userService.BanUserAsync(userId))
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