using ActPro.DAL;

namespace ActPro.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<bool> ToggleRoleAsync(string userId, string roleName);
        Task<bool> BanUserAsync(string userId);
    }
}