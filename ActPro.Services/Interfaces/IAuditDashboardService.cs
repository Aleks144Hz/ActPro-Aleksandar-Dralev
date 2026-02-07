using ActPro.DAL.Entities;

namespace ActPro.Services.Interfaces
{
    public interface IAuditDashboardService
    {
        Task LogAsync(string action, string entity, string entityId, string details);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    }
}
