using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models;

namespace ActPro.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationViewModel> GetReservationIndexModelAsync(int placeId, string userId);
        Task<(bool success, string message)> BookAsync(int placeId, DateTime date, string timeSlot, ApplicationUser user);
        Task<List<string>> GetOccupiedSlotsAsync(int placeId, DateTime date);
        Task<(IEnumerable<Reservation> reservations, int totalCount)> GetUserReservationsAsync(string userId, int page, int pageSize, string filter);
        Task<(bool success, string message)> CancelReservationAsync(int reservationId, string userId);
        Task<(bool success, string message)> AddReviewAsync(int placeId, string userId, string commentText, int rating);
        Task<(bool success, string message)> EditReviewAsync(int commentId, string userId, string commentText, int rating);
        Task<(bool success, string message)> DeleteReviewAsync(int reviewId, string userId);
        Task<List<Comment>> GetUserReviewsAsync(string userId);
    }
}