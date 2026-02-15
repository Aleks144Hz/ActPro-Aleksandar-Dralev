using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class AuditDashboardService(ApplicationDbContext context) : IAuditDashboardService
    {
        //---Logs---
        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await context.AuditLogs.OrderByDescending(l => l.Id).ToListAsync();
        }
        public async Task<AuditLogsViewModel> GetAuditLogsIndexModelAsync()
        {
            var logs = await context.AuditLogs
            .OrderByDescending(l => l.Id)
            .ToListAsync();

            return new AuditLogsViewModel
            {
                Logs = logs.Select(l => new AuditLogItemViewModel
                {
                    Id = l.Id,
                    CreatedAt = l.CreatedAt,
                    UserEmail = l.UserEmail,
                    Action = l.Action,
                    Details = l.Details,
                    IpAddress = l.IpAddress
                }).ToList()
            };
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
