using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAuditService auditService)
        {
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<bool> ToggleRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
                await _auditService.LogAsync("Edit User", "User", userId, $"Премахната роля {roleName} на: {user.FirstName} {user.LastName}");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, roleName);
                await _auditService.LogAsync("Edit User", "User", userId, $"Добавена роля {roleName} на: {user.FirstName} {user.LastName}");
            }
            return true;
        }

        public async Task<bool> BanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var bannedEntry = new BannedUser
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                BannedAt = DateTime.Now
            };

            _context.BannedUsers.Add(bannedEntry);
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Ban User", "User", userId, $"Блокиран и изтрит: {user.FirstName} {user.LastName} | Email: {user.Email}");
                return true;
            }
            return false;
        }
    }
}