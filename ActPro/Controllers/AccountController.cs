using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- ACCOUNT / PROFILE (Index) ---
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
            .Include(u => u.Favorites)
            .ThenInclude(f => f.Place)
            .ThenInclude(p => p.PlaceImages) 
            .Include(u => u.Favorites)
            .ThenInclude(f => f.Place)
            .ThenInclude(p => p.City)     
            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return View(user);
        }

        // --- LOGIN ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("/Index");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Грешна парола.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Потребителят не е намерен.");
            }
            return View(model);
        }

        // --- REGISTER ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // --- LOGOUT ---
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // --- DELETE PROFILE ---
        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDelete()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile(DeleteProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Favorites).ThenInclude(f => f.Place)
                .Include(u => u.Favorites).ThenInclude(f => f.Place).ThenInclude(p => p.PlaceImages)
                .Include(u => u.Favorites).ThenInclude(f => f.Place).ThenInclude(p => p.City)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (!ModelState.IsValid)
            {
                return View("Index", user);
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordCorrect)
            {
                ModelState.AddModelError("Password", "Грешна парола, моля опитайте отново!");
                return View("Index", user);
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Грешка при изтриване.");
            return View("Index", user);
        }

        // --- EDIT PROFILE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            string oldFileName = user.ProfilePicturePath;
            bool isNewPictureUploaded = false;
            user.FirstName = !string.IsNullOrWhiteSpace(model.FirstName) ? model.FirstName : user.FirstName;
            user.LastName = !string.IsNullOrWhiteSpace(model.LastName) ? model.LastName : user.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                var filePath = Path.Combine(folderPath, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }
                user.ProfilePicturePath = newFileName;
                isNewPictureUploaded = true;
            }
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (isNewPictureUploaded && !string.IsNullOrEmpty(oldFileName))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles", oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Неуспешно триене: " + ex.Message);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        //--- FAVORITES ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int placeId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.PlaceId == placeId && f.AspNetUserId == userId);

            if (existingFavorite != null)
            {
                _context.Favorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = false });
            }

            var favorite = new Favorite { AspNetUserId = userId, PlaceId = placeId };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorite = true });
        }
    }
}