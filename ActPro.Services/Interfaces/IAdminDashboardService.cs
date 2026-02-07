using ActPro.Domain.Models.Areas;

namespace ActPro.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminStatsAsync();
        Task<object> GetChartDataAsync(string period);
    }
}
