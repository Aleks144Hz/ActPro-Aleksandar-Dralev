using ActPro.DAL;
using ActPro.Domain.Models;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Controllers
{
    public class HomeController(IHomeService homeService, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IAuditService auditService, IEmailSender emailSender) : Controller
    {
        //---HOME PAGE---
        public async Task<IActionResult> Index()
        {
            var viewModel = await homeService.GetHomeViewModelAsync();
            return View(viewModel);
        }

        //---SET LANGUAGE---
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        //---NEWS PAGE---
        [HttpGet]
        public async Task<IActionResult> News(int page = 1)
        {
            int pageSize = 6;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (newsItems, totalPages) = await homeService.GetNewsPagedAsync(page, pageSize, userId);

            var viewModel = new NewsViewModel
            {
                NewsItems = newsItems,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(viewModel);
        }

        //---CREATE & DELETE NEWS---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNews(string title, string content, IFormFile? imageFile)
        {
            var user = await userManager.GetUserAsync(User);
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
            {
                await homeService.CreateNewsAsync(title, content, imageFile, webHostEnvironment.WebRootPath);
                await auditService.LogAsync("Create News", "User", user.Id, $"{NewsIsPublished} \"{title}\"");
                TempData["SuccessMessage"] = NewsIsPublishedSuccessfully;
                return RedirectToAction(nameof(News));
            }
            return RedirectToAction(nameof(News));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var user = await userManager.GetUserAsync(User);
            await homeService.DeleteNewsAsync(id, webHostEnvironment.WebRootPath);
            await auditService.LogAsync("Delete News", "User", user.Id, $"{NewsDeleted} {id}");
            TempData["SuccessMessage"] = NewsDeletedSuccessfully;
            return RedirectToAction(nameof(News));
        }

        //---LIKE NEWS---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeNews(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false });

            var result = await homeService.LikeNewsAsync(id, userId);

            return Json(new
            {
                success = true,
                likes = result.likes,
                isLiked = result.isLiked
            });
        }

        //---CONTACT SUPPORT---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactSupport(SupportTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = FillInAllFields;
                return View("Support", model);
            }

            try
            {
                await emailSender.SendSupportTicketAsync(model);
                TempData["Success"] = TicketSendSuccessfully;
                return RedirectToAction("Support");
            }
            catch
            {
                TempData["Error"] = Error;
                return View("Support", model);
            }
        }
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Support() => View();
    }
}