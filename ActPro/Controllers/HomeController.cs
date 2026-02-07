using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IHomeService homeService, IWebHostEnvironment webHostEnvironment)
        {
            _homeService = homeService;
            _webHostEnvironment = webHostEnvironment;
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
            int pageSize = 3;
            var (news, totalPages) = await _homeService.GetNewsPagedAsync(page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(news);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateNews() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNews(News news, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                await _homeService.CreateNewsAsync(news, imageFile, _webHostEnvironment.WebRootPath);
                return RedirectToAction(nameof(News));
            }
            return View(news);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            await _homeService.DeleteNewsAsync(id);
            return RedirectToAction(nameof(News));
        }
    }
}