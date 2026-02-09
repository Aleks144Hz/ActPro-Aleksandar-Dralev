using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Repository;
using ActPro.Models.User;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ActPro.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUser> _userRepo;
        private readonly IRepository<Favorite> _favRepo;
        private readonly IRepository<Reservation> _resRepo;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IRepository<BannedUser> _banRepo;
        private readonly IConfiguration _configuration;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IRepository<ApplicationUser> userRepo, IRepository<Favorite> favRepo, IRepository<Reservation> resRepo, IRepository<Comment> commentRepo, IRepository<BannedUser> banRepo, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepo = userRepo;
            _favRepo = favRepo;
            _resRepo = resRepo;
            _commentRepo = commentRepo;
            _banRepo = banRepo;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> GetUserFullProfileAsync(string userId) => await _userRepo.AllAsNoTracking()
        .Include(u => u.Favorites).ThenInclude(f => f.Place).ThenInclude(p => p.PlaceImages)
        .Include(u => u.Favorites).ThenInclude(f => f.Place).ThenInclude(p => p.City)
        .FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<ApplicationUser> GetUserByIdAsync(string userId) => await _userManager.FindByIdAsync(userId);

        public async Task<(int resCount, int revCount)> GetUserActivityStatsAsync(string userId)
        {
            var resCount = await _resRepo.AllAsNoTracking().CountAsync(r => r.AspNetUserId == userId);
            var revCount = await _commentRepo.AllAsNoTracking().CountAsync(c => c.AspNetUserId == userId);
            return (resCount, revCount);
        }

        public async Task<bool> IsUserBannedAsync(string email, string phone = null) => await _banRepo.AllAsNoTracking().AnyAsync(b => b.Email == email || phone != null && b.Phone == phone);

        public async Task<bool> VerifyReCaptchaAsync(string response)
        {
            if (string.IsNullOrEmpty(response)) return false;
            string secret = _configuration["GoogleReCaptcha:SecretKey"];
            using var client = new HttpClient();
            var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}";
            var result = await client.PostAsync(verifyUrl, null);
            var jsonString = await result.Content.ReadAsStringAsync();
            return jsonString.Contains("\"success\": true");
        }

        public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return SignInResult.Failed;
            return await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedOn = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            return result;
        }

        public async Task<IdentityResult> UpdateProfileAsync(string userId, EditProfileViewModel model, string webRootPath)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed();

            user.FirstName = !string.IsNullOrWhiteSpace(model.FirstName) ? model.FirstName : user.FirstName;
            user.LastName = !string.IsNullOrWhiteSpace(model.LastName) ? model.LastName : user.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var folderPath = Path.Combine(webRootPath, "images", "profiles");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                var filePath = Path.Combine(folderPath, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var oldPath = Path.Combine(folderPath, user.ProfilePicturePath);
                    if (File.Exists(oldPath)) try { File.Delete(oldPath); } catch { }
                }

                user.ProfilePicturePath = newFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            return result;
        }

        public async Task<IdentityResult> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed();

            var userReservations = _resRepo.All().Where(r => r.AspNetUserId == userId);
            foreach (var res in userReservations)
            {
                _resRepo.Delete(res);
            }
            await _resRepo.SaveChangesAsync();

            return await _userManager.DeleteAsync(user);
        }

        public async Task<(bool isFavorite, string message)> ToggleFavoriteAsync(string userId, int placeId)
        {
            var existingFavorite = await _favRepo.All()
                .FirstOrDefaultAsync(f => f.PlaceId == placeId && f.AspNetUserId == userId);

            if (existingFavorite != null)
            {
                _favRepo.Delete(existingFavorite);
                await _favRepo.SaveChangesAsync();
                return (false, "Премахнато от любими");
            }

            var favorite = new Favorite { AspNetUserId = userId, PlaceId = placeId };
            await _favRepo.AddAsync(favorite);
            await _favRepo.SaveChangesAsync();
            return (true, "Добавено в любими!");
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            return result;
        }

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();
    }
}