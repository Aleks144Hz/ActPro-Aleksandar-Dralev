using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;

namespace ActPro.Services.Interfaces
{
    internal interface IReservationDashboardService
    {
        Task<ReservationsIndexViewModel> GetReservationsIndexModelAsync(string? ownerId = null);
        Task<Reservation?> GetByIdAsync(int id);
        Task<bool> DeleteReservationAsync(int id, string? ownerId = null);
        Task<bool> UpdateReservationTimeAsync(int id, TimeOnly newTime, string? ownerId = null);
        Task<bool> CreateManualReservationAsync(int placeId, string customerNote, DateOnly date, TimeOnly time);
    }
}
