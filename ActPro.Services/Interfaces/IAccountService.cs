using ActPro.DAL;
using ActPro.Models.User;
using Microsoft.AspNetCore.Identity;

namespace ActPro.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ApplicationUser> GetUserFullProfileAsync(string userId);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<(int resCount, int revCount)> GetUserActivityStatsAsync(string userId);
        Task<bool> IsUserBannedAsync(string email, string phone = null);
        Task<bool> VerifyReCaptchaAsync(string captchaResponse);
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> UpdateProfileAsync(string userId, EditProfileViewModel model, string webRootPath);
        Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<IdentityResult> DeleteAccountAsync(string userId);
        Task<(bool isFavorite, string message)> ToggleFavoriteAsync(string userId, int placeId);
    }
}