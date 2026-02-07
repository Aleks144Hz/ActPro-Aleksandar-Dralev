using ActPro.DAL.Entities;

namespace ActPro.Services.Interfaces
{
    internal interface IReservationDashboardService
    {
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task<IEnumerable<Reservation>> GetOwnerReservationsAsync(string ownerId);
        Task<Reservation?> GetByIdAsync(int id);
        Task<bool> DeleteReservationAsync(int id, string? ownerId = null);
        Task<bool> UpdateReservationTimeAsync(int id, TimeOnly newTime, string? ownerId = null);
    }
}
