using ActPro.Domain.Models.Areas;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static ActPro.Helpers.MessageConstants;

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
            var viewModel = new UsersIndexViewModel
            {
                Users = await userService.GetAllUsersWithRolesAsync()
            };
            return View(viewModel);
        }

        //--- Toggle Admin Role ---
        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            if (await userService.ToggleRoleAsync(userId, "Admin"))
            {
                TempData["Success"] = AdminRuleChanged;
            }
            return RedirectToAction(nameof(Index));
        }

        //--- Toggle Owner Role ---
        [HttpPost]
        public async Task<IActionResult> ToggleOwner(string userId)
        {
            if (await userService.ToggleRoleAsync(userId, "Owner"))
            {
                TempData["Success"] = OwnerRuleChanged;
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
                TempData["Success"] = UserBanned;
            }
            else
            {
                TempData["Error"] = Error;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}