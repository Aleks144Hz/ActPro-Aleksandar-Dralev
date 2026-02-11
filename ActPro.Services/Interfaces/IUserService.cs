using ActPro.DAL;
using ActPro.Domain.Models.Areas;

namespace ActPro.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IEnumerable<UserItemViewModel>> GetAllUsersWithRolesAsync();
        Task<bool> ToggleRoleAsync(string userId, string roleName);
        Task<bool> BanUserAsync(string userId);
    }
}