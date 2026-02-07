using ActPro.DAL;
using ActPro.Models.User;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(
            IAccountService accountService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _accountService = accountService;
            _auditService = auditService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- ACCOUNT / PROFILE (Index) ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _accountService.GetUserFullProfileAsync(userId);

            if (user == null) return NotFound();

            var stats = await _accountService.GetUserActivityStatsAsync(userId);
            var viewModel = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicturePath = user.ProfilePicturePath,
                Credits = user.Credits,
                Favorites = user.Favorites,
            };
            ViewBag.ResCount = stats.resCount;
            ViewBag.RevCount = stats.revCount;

            return View(viewModel);
        }

        // --- LOGIN ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (await _accountService.IsUserBannedAsync(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Акаунтът ви е блокиран.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            var result = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                await _auditService.LogAsync("User Login", "User", user.Id, "Потребителят влезе в системата.");
                TempData["Success"] = "Добре дошли!";
                return RedirectToAction("Index", "Home");
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Password", NotValidPassword);
            }
            else
            {
                ModelState.AddModelError("Email", UserIsNotRegistered);
            }

            return View(model);
        }

        // --- REGISTER ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (await _accountService.IsUserBannedAsync(model.Email, model.PhoneNumber))
            {
                ModelState.AddModelError("", "Този имейл е блокиран.");
                return View(model);
            }

            string captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await _accountService.VerifyReCaptchaAsync(captchaResponse))
            {
                ModelState.AddModelError("CaptchaResponse", "Моля потвърдете, че не сте робот.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterAsync(model);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    TempData["Success"] = "Регистрацията е успешна! Добре дошли.";
                    await _auditService.LogAsync("Create User", "User", user.Id, "Нов потребител се регистрира");
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    if (error.Code.Contains("Password"))
                        ModelState.AddModelError("Password", error.Description);
                    else
                        ModelState.AddModelError("Email", error.Description);
                }
            }
            return View(model);
        }

        // --- EDIT PROFILE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _accountService.UpdateProfileAsync(userId, model, _webHostEnvironment.WebRootPath);

            if (result.Succeeded)
            {
                TempData["Success"] = SuccessfulUserEdit;
                await _auditService.LogAsync("Update Settings", "User", userId, "Потребителят обнови профилните си данни.");
            }

            return RedirectToAction("Settings");
        }

        // --- CHANGE PASSWORD ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _accountService.GetUserByIdAsync(userId);

            if (!ModelState.IsValid)
            {
                ViewData["ShowChangePasswordModal"] = true;
                return View("Settings", user);
            }

            var result = await _accountService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("Update Settings", "User", userId, "Потребителят смени паролата си успешно.");
                TempData["Success"] = "Паролата е променена успешно!";
                return RedirectToAction("Settings");
            }

            bool hasPasswordError = false;
            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                    ModelState.AddModelError("OldPassword", "Въвели сте грешна парола.");
                else if (error.Code.Contains("Password") && !hasPasswordError)
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                    hasPasswordError = true;
                }
            }

            ViewData["ShowChangePasswordModal"] = true;
            return View("Settings", user);
        }

        // --- DELETE PROFILE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile(DeleteProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _accountService.GetUserFullProfileAsync(userId);

            if (!ModelState.IsValid)
            {
                ViewData["ShowDeleteModal"] = true;
                return View("Settings", user);
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordCorrect)
            {
                ModelState.AddModelError("Password", "Грешна парола, моля опитайте отново!");
                ViewData["ShowDeleteModal"] = true;
                return View("Settings", user);
            }

            var result = await _accountService.DeleteAccountAsync(userId);
            if (result.Succeeded)
            {
                await _accountService.LogoutAsync();
                TempData["Success"] = SuccessfulDeletedAccount;
                await _auditService.LogAsync("Delete User", "User", userId, "Потребителят сам изтри профила си");
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Грешка при изтриване.");
            ViewData["ShowDeleteModal"] = true;
            return View("Index", user);
        }

        // --- FAVORITES ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleFavorite(int placeId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.ToggleFavoriteAsync(userId, placeId);
            return Json(new { success = true, isFavorite = result.isFavorite, message = result.message });
        }

        // --- LOGOUT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            TempData["Success"] = "Довиждане!";
            return RedirectToAction("Index", "Home");
        }

        // --- SETTINGS ---
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _accountService.GetUserByIdAsync(userId);

            if (user == null) return NotFound();
            var viewModel = new ProfileSettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicturePath = user.ProfilePicturePath,
                CreatedOn = user.CreatedOn
            };
            return View(viewModel);
        }
    }
}