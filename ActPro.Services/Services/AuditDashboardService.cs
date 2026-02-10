using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class AuditDashboardService(ApplicationDbContext context) : IAuditDashboardService
    {
        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await context.AuditLogs.OrderByDescending(l => l.Id).ToListAsync();
        }

        public async Task LogAsync(string action, string entity, string entityId, string details)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityName = entity,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.Now
            };

            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}
