using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Account;
using ActPro.Domain.Repository;
using ActPro.Models.User;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace ActPro.Services.Services
{
    public class AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IRepository<ApplicationUser> userRepo,
        IRepository<Favorite> favRepo, IRepository<Reservation> resRepo, IRepository<Comment> commentRepo, IRepository<BannedUser> banRepo, IConfiguration configuration) : IAccountService
    {
        //--- GET USER PROFILE ---
        public async Task<ApplicationUser> GetUserFullProfileAsync(string userId) => await userRepo.AllAsNoTracking()
        .Include(u => u.Favorites)
        .ThenInclude(f => f.Place)
        .ThenInclude(p => p.PlaceImages)
        .Include(u => u.Favorites)
        .ThenInclude(f => f.Place)
        .ThenInclude(p => p.City)
        .FirstOrDefaultAsync(u => u.Id == userId);
        
        public async Task<ApplicationUser> GetUserByIdAsync(string userId) => await userManager.FindByIdAsync(userId);

        public async Task<(int resCount, int revCount)> GetUserActivityStatsAsync(string userId)
        {
            var resCount = await resRepo.AllAsNoTracking().CountAsync(r => r.AspNetUserId == userId);
            var revCount = await commentRepo.AllAsNoTracking().CountAsync(c => c.AspNetUserId == userId);
            return (resCount, revCount);
        }

        public async Task<bool> IsUserBannedAsync(string email, string phone = null) => await banRepo.AllAsNoTracking().AnyAsync(b => b.Email == email || phone != null && b.Phone == phone);

        public async Task<bool> VerifyReCaptchaAsync(string response)
        {
            if (string.IsNullOrEmpty(response)) return false;
            string secret = configuration["GoogleReCaptcha:SecretKey"];
            using var client = new HttpClient();
            var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}";
            var result = await client.PostAsync(verifyUrl, null);
            var jsonString = await result.Content.ReadAsStringAsync();
            return jsonString.Contains("\"success\": true");
        }

        //--- LOGIN ---
        public async Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return Microsoft.AspNetCore.Identity.SignInResult.Failed;
            return await signInManager.PasswordSignInAsync(user, password, rememberMe, false);
        }

        //--- REGISTER ---
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

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                await signInManager.SignInAsync(user, isPersistent: false);
            }
            return result;
        }

        //--- UPDATE PROFILE ---
        public async Task<IdentityResult> UpdateProfileAsync(string userId, EditProfileViewModel model, string webRootPath)
        {
            var user = await userManager.FindByIdAsync(userId);
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

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
            }
            return result;
        }

        //--- DELETE ACOUNT ---
        public async Task<IdentityResult> DeleteAccountAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed();

            var userReservations = resRepo.All().Where(r => r.AspNetUserId == userId);
            foreach (var res in userReservations)
            {
                resRepo.Delete(res);
            }
            await resRepo.SaveChangesAsync();

            return await userManager.DeleteAsync(user);
        }

        //--- TOGGLE FAVORITE ---
        public async Task<(bool isFavorite, string message)> ToggleFavoriteAsync(string userId, int placeId)
        {
            var existingFavorite = await favRepo.All()
                .FirstOrDefaultAsync(f => f.PlaceId == placeId && f.AspNetUserId == userId);

            if (existingFavorite != null)
            {
                favRepo.Delete(existingFavorite);
                await favRepo.SaveChangesAsync();
                return (false, "Премахнато от любими");
            }

            var favorite = new Favorite { AspNetUserId = userId, PlaceId = placeId };
            await favRepo.AddAsync(favorite);
            await favRepo.SaveChangesAsync();
            return (true, "Добавено в любими!");
        }

        //--- CHANGE PASSWORD ---
        public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
            }
            return result;
        }

        //--- PASSWORD RESET ---
        public async Task<string> GeneratePasswordResetLinkAsync(string email, string scheme, IUrlHelper url)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user))) return null;

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return url.Action("ForgotPassword", "Account", new { email = email, token = encodedToken }, scheme);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return IdentityResult.Failed();

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            return await userManager.ResetPasswordAsync(user, decodedToken, model.Password);
        }
        public async Task LogoutAsync() => await signInManager.SignOutAsync();
    }
}