using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class AuditDashboardService : IAuditDashboardService
    {
        private readonly ApplicationDbContext _context;
        public AuditDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await _context.AuditLogs.OrderByDescending(l => l.Id).ToListAsync();
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

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
