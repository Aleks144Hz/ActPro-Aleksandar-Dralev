using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.Domain;
using ActPro.Domain.Models;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ActPro.Controllers
{
    public class HomeController(IHomeService homeService, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IAuditService auditService, IEmailSender emailSender, ApplicationDbContext context) : Controller
    {
        //---HOME PAGE---
        public async Task<IActionResult> Index()
        {
            var viewModel = await homeService.GetHomeViewModelAsync();
            return View(viewModel);
        }

        private string TranslateSearchParameter(string value, string targetCulture)
        {
            if (string.IsNullOrEmpty(value)) return value;

            bool toEnglish = targetCulture == "en";
            
            if (toEnglish)
            {
                var cities = context.Cities.ToList();
                var city = cities.FirstOrDefault(c => c.Name != null && c.Name.ToLower() == value.ToLower());
                if (city != null && !string.IsNullOrEmpty(city.NameEn)) return city.NameEn;
                
                var activities = context.Activities.ToList();
                var activity = activities.FirstOrDefault(a => a.Name != null && a.Name.ToLower() == value.ToLower());
                if (activity != null && !string.IsNullOrEmpty(activity.NameEn)) return activity.NameEn;
            }
            else
            {
                var cities = context.Cities.ToList();
                var city = cities.FirstOrDefault(c => c.NameEn != null && c.NameEn.ToLower() == value.ToLower());
                if (city != null && !string.IsNullOrEmpty(city.Name)) return city.Name;
                
                var activities = context.Activities.ToList();
                var activity = activities.FirstOrDefault(a => a.NameEn != null && a.NameEn.ToLower() == value.ToLower());
                if (activity != null && !string.IsNullOrEmpty(activity.Name)) return activity.Name;
            }

            return value;
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

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("/Search"))
            {
                try
                {
                    var queryStart = returnUrl.IndexOf("?");
                    if (queryStart >= 0)
                    {
                        var query = returnUrl.Substring(queryStart + 1);
                        var parameters = System.Web.HttpUtility.ParseQueryString(query);
                        
                        var cityParam = parameters["city"];
                        var activityParam = parameters["activity"];
                        
                        var newParams = new List<string>();
                        
                        if (!string.IsNullOrEmpty(cityParam))
                        {
                            var translatedCity = TranslateSearchParameter(cityParam, culture);
                            newParams.Add($"city={Uri.EscapeDataString(translatedCity)}");
                        }
                        
                        if (!string.IsNullOrEmpty(activityParam))
                        {
                            var translatedActivity = TranslateSearchParameter(activityParam, culture);
                            newParams.Add($"activity={Uri.EscapeDataString(translatedActivity)}");
                        }
                        
                        var baseUrl = returnUrl.Substring(0, queryStart);
                        if (baseUrl.Contains("/Search/Index"))
                            baseUrl = "/Search";
                        
                        returnUrl = newParams.Any() 
                            ? $"{baseUrl}?{string.Join("&", newParams)}"
                            : baseUrl;
                    }
                    else if (returnUrl.Contains("/Search/Index"))
                    {
                        returnUrl = "/Search";
                    }
                }
                catch
                {
                    returnUrl = "/Search";
                }
            }

            return LocalRedirect(returnUrl ?? "/");
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
        public async Task<IActionResult> CreateNews(string title, string content, string titleEn, string contentEn, IFormFile? imageFile)
        {
            var user = await userManager.GetUserAsync(User);
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
            {
                await homeService.CreateNewsAsync(title, content, titleEn, contentEn, imageFile, webHostEnvironment.WebRootPath);
                await auditService.LogAsync("Create News", "User", user.Id, $"Публикувана е новина: \"{title}\"");
                TempData["SuccessMessage"] = DomainResources.NewsIsPublishedSuccessfully;
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
            await auditService.LogAsync("Delete News", "User", user.Id, $"Изтрита новина с ID: {id}");
            TempData["SuccessMessage"] = DomainResources.NewsDeletedSuccessfully;
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
                TempData["Error"] = DomainResources.FillInAllFields;
                return View("Support", model);
            }

            try
            {
                await emailSender.SendSupportTicketAsync(model);
                TempData["Success"] = DomainResources.TicketSendSuccessfully;
                return RedirectToAction("Support");
            }
            catch
            {
                TempData["Error"] = DomainResources.Error;
                return View("Support", model);
            }
        }
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Support() => View();
    }
}