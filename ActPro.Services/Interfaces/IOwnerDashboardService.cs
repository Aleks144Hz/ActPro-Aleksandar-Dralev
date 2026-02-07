using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;

namespace ActPro.Services.Interfaces
{
    public interface IOwnerDashboardService
    {
        Task<OwnerDashboardViewModel> GetOwnerStatsAsync(string userId);
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<IEnumerable<Activity>> GetActivitiesAsync();
    }
}
