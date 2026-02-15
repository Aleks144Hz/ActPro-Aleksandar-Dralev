using ActPro.DAL;
using ActPro.Domain.Models.Account;
using ActPro.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ApplicationUser> GetUserFullProfileAsync(string userId);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<(int resCount, int revCount)> GetUserActivityStatsAsync(string userId);
        Task<bool> IsUserBannedAsync(string email, string phone = null);
        Task<bool> VerifyReCaptchaAsync(string captchaResponse);
        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<IdentityResult> UpdateProfileAsync(string userId, EditProfileViewModel model, string webRootPath);
        Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<IdentityResult> DeleteAccountAsync(string userId);
        Task<string> GeneratePasswordResetLinkAsync(string email, string scheme, IUrlHelper url);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<(bool isFavorite, string message)> ToggleFavoriteAsync(string userId, int placeId);
        Task LogoutAsync();

    }
}