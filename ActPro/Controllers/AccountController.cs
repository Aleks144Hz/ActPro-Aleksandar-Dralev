using ActPro.DAL;
using ActPro.Domain.Models.Account;
using ActPro.Models.User;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static ActPro.Helpers.MessageConstants;

namespace ActPro.Controllers
{
    [Authorize]
    public class AccountController(IAccountService accountService, IAuditService auditService, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender) : Controller
    {
        // --- ACCOUNT / PROFILE (Index) ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);
            var user = await accountService.GetUserFullProfileAsync(userId);

            if (user == null) return NotFound();

            var stats = await accountService.GetUserActivityStatsAsync(userId);
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
            if (await accountService.IsUserBannedAsync(model.Email))
            {
                ModelState.AddModelError(string.Empty, UserAccountIsBanned);
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            var result = await accountService.LoginAsync(model.Email, model.Password, model.RememberMe);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                await auditService.LogAsync("User Login", "User", user.Id, UserLoggedInSuccessfully);
                TempData["Success"] = Welcome;
                return RedirectToAction("Index", "Home");
            }

            var userExists = await userManager.FindByEmailAsync(model.Email);
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
            var accepted = Request.Form["AcceptTerms"] == "true";

            if (!accepted)
            {
                ModelState.AddModelError("AcceptTerms", AgreeWithTerms);
            }

            if (await accountService.IsUserBannedAsync(model.Email, model.PhoneNumber))
            {
                ModelState.AddModelError("", EmailIsBanned);
                return View(model);
            }

            string captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await accountService.VerifyReCaptchaAsync(captchaResponse))
            {
                ModelState.AddModelError("CaptchaResponse", ProveYouAreNotRobot);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var result = await accountService.RegisterAsync(model);

                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                    await emailSender.SendEmailAsync(user.Email, AccountApproved, confirmationLink);
                    TempData["Success"] = SuccsessfullRegistration;
                    await auditService.LogAsync("Create User", "User", user.Id, NewUserRegistered);
                    return RedirectToAction("RegisterConfirmation", new { email = model.Email });
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

        // --- FORGOT / RESET PASSWORD ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string token = null, string email = null)
        {
            if (token != null && email != null)
            {
                return View(new ResetPasswordViewModel { Token = token, Email = email });
            }
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var callbackUrl = await accountService.GeneratePasswordResetLinkAsync(model.Email, Request.Scheme, Url);

            if (callbackUrl != null)
            {
                await emailSender.SendPasswordResetAsync(model.Email, callbackUrl);
            }

            TempData["Success"] = ValidEmailAddress;
            return RedirectToAction("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View("ForgotPassword", model);

            var result = await accountService.ResetPasswordAsync(model);
            if (result.Succeeded)
            {
                TempData["Success"] = PasswordResetSuccess;
                return RedirectToAction("Login");
            }


            bool hasPasswordError = false;
            foreach (var error in result.Errors)
            {
                if (error.Code.Contains("Password") && !hasPasswordError)
                {
                    ModelState.AddModelError("resetModel.Password", error.Description);
                    hasPasswordError = true;
                }
            }
            return View("ForgotPassword", model);
        }

        // --- EMAIL CONFIRMATION ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Success"] = EmailApprovedSuccess;
                return RedirectToAction("Login");
            }

            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
            {
                TempData["Info"] = EmailAlreadyApproved;
                return RedirectToAction("Login");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token = token }, Request.Scheme);

            await emailSender.SendEmailAsync(user.Email, AccountEmailApprovel, confirmationLink);

            return RedirectToAction("RegisterConfirmation", new { email = user.Email });
        }

        // --- FAVORITES ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleFavorite(int placeId)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await accountService.ToggleFavoriteAsync(userId, placeId);
            return Json(new { success = true, isFavorite = result.isFavorite, message = result.message });
        }

        // --- SETTINGS ---
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = userManager.GetUserId(User);
            var user = await accountService.GetUserByIdAsync(userId);

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

        // --- EDIT PROFILE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userId = userManager.GetUserId(User);
            var result = await accountService.UpdateProfileAsync(userId, model, webHostEnvironment.WebRootPath);

            if (result.Succeeded)
            {
                TempData["Success"] = SuccessfulUserEdit;
                await auditService.LogAsync("Update Settings", "User", userId, AccountInfoUpadate);
            }

            return RedirectToAction("Settings");
        }

        // --- CHANGE PASSWORD ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = userManager.GetUserId(User);
            var user = await accountService.GetUserByIdAsync(userId);

            if (!ModelState.IsValid)
            {
                ViewData["ShowChangePasswordModal"] = true;
                return View("Settings", MapToSettingsViewModel(user));
            }

            var result = await accountService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await auditService.LogAsync("Update Settings", "User", userId, UserUpdatePasswordSuccess);
                TempData["Success"] = PasswordUpdatedSuccessfully;
                return RedirectToAction("Settings");
            }

            bool hasPasswordError = false;
            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                    ModelState.AddModelError("OldPassword", WrongPassword);
                else if (error.Code.Contains("Password") && !hasPasswordError)
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                    hasPasswordError = true;
                }
            }

            ViewData["ShowChangePasswordModal"] = true;
            return View("Settings", MapToSettingsViewModel(user));
        }

        // --- DELETE PROFILE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile(DeleteProfileViewModel model)
        {
            var userId = userManager.GetUserId(User);
            var user = await accountService.GetUserFullProfileAsync(userId);

            if (!ModelState.IsValid)
            {
                ViewData["ShowDeleteModal"] = true;
                return View("Settings", MapToSettingsViewModel(user));
            }

            var isPasswordCorrect = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordCorrect)
            {
                ModelState.AddModelError("Password", WrongPassword);
                ViewData["ShowDeleteModal"] = true;
                return View("Settings", MapToSettingsViewModel(user));
            }

            var result = await accountService.DeleteAccountAsync(userId);
            if (result.Succeeded)
            {
                await emailSender.SendProfileDeletedAsync(user.Email, user.FirstName);
                await accountService.LogoutAsync();
                TempData["Success"] = SuccessfulDeletedAccount;
                await auditService.LogAsync("Delete User", "User", userId, UserDeleteAccountSuccess);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, Error);
            ViewData["ShowDeleteModal"] = true;
            return View("Index", MapToSettingsViewModel(user));
        }

        // --- LOGOUT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await accountService.LogoutAsync();
            TempData["Success"] = GoodBye;
            return RedirectToAction("Index", "Home");
        }

        // --- PRIVATE HELPER METHODS ---
        private ProfileSettingsViewModel MapToSettingsViewModel(ApplicationUser user)
        {
            return new ProfileSettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicturePath = user.ProfilePicturePath,
                CreatedOn = user.CreatedOn
            };
        }
    }
}