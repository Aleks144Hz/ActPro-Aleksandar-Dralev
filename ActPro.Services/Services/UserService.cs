using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAuditService auditService) : IUserService
    {
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await userManager.Users.ToListAsync();
        }

        public async Task<bool> ToggleRoleAsync(string userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isInRole = await userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
            {
                await userManager.RemoveFromRoleAsync(user, roleName);
                await auditService.LogAsync("Edit User", "User", userId, $"Премахната роля {roleName} на: {user.FirstName} {user.LastName}");
            }
            else
            {
                await userManager.AddToRoleAsync(user, roleName);
                await auditService.LogAsync("Edit User", "User", userId, $"Добавена роля {roleName} на: {user.FirstName} {user.LastName}");
            }
            return true;
        }

        public async Task<bool> BanUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var bannedEntry = new BannedUser
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                BannedAt = DateTime.Now
            };

            context.BannedUsers.Add(bannedEntry);
            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await context.SaveChangesAsync();
                await auditService.LogAsync("Ban User", "User", userId, $"Блокиран и изтрит: {user.FirstName} {user.LastName} | Email: {user.Email}");
                return true;
            }
            return false;
        }
    }
}