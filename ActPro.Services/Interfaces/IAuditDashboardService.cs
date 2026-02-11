using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;

namespace ActPro.Services.Interfaces
{
    public interface IAuditDashboardService
    {
        Task<AuditLogsViewModel> GetAuditLogsIndexModelAsync();
        Task LogAsync(string action, string entity, string entityId, string details);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    }
}
