using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services
{
    public class AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) : IAuditService
    {
        public async Task LogAsync(string action, string entity, string? entityId, string details)
        {
            var user = httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = user?.Identity?.Name;
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityName = entity,
                EntityId = entityId,
                Details = details,
                IpAddress = ip,
                CreatedAt = DateTime.Now
            };

            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}
