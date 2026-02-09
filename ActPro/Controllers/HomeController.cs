using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ActPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly IAuditService _auditService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;


        public HomeController(IHomeService homeService, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _homeService = homeService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _auditService = auditService;
        }

        //---HOME PAGE---
        public async Task<IActionResult> Index()
        {
            var viewModel = await _homeService.GetHomeViewModelAsync();
            return View(viewModel);
        }

        //---NEWS PAGE---
        public async Task<IActionResult> News(int page = 1)
        {
            int pageSize = 6;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (newsItems, totalPages) = await _homeService.GetNewsPagedAsync(page, pageSize, userId);

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
        public async Task<IActionResult> CreateNews(string Title, string Content, IFormFile? imageFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Content))
            {
                var news = new News
                {
                    Title = Title,
                    Content = Content,
                    CreatedAt = DateTime.Now
                };

                await _homeService.CreateNewsAsync(news, imageFile, _webHostEnvironment.WebRootPath);
                await _auditService.LogAsync("Create News", "User", user.Id, $"Публикувана е нова новина: \"{Title}\"");
                TempData["SuccessMessage"] = "Новината е публикувана успешно!";
                return RedirectToAction(nameof(News));
            }
            return RedirectToAction(nameof(News));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _homeService.DeleteNewsAsync(id, _webHostEnvironment.WebRootPath);
            await _auditService.LogAsync("Delete News", "User", user.Id, $"Изтрита новина с ID: {id}");
            TempData["SuccessMessage"] = "Новината е успешно изтрита";
            return RedirectToAction(nameof(News));
        }

        //---LIKE NEWS---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeNews(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false });

            var result = await _homeService.LikeNewsAsync(id, userId);

            return Json(new
            {
                success = true,
                likes = result.likes,
                isLiked = result.isLiked
            });
        }
    }
}