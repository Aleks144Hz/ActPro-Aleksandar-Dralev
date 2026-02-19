using ActPro.Domain.Models;

namespace ActPro.Services.Interfaces
{
    public interface IEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        Task SendBookingConfirmationAsync(string email, string firstName, string placeName, string date, string timeSlot);
        Task SendBookingCancellationAsync(string email, string firstName, string placeName, string date, string timeSlot);
        Task SendReservationTimeChangedAsync(string email, string firstName, string placeName, string oldTime, string newTime, string date);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task SendProfileDeletedAsync(string email, string firstName);
        Task SendPlaceApprovedAsync(string email, string firstName, string placeName);
        Task SendPlaceRejectedAsync(string email, string firstName, string placeName);
        Task SendPlaceDeletedAsync(string email, string firstName, string placeName);
        Task SendNewBookingNotificationAsync(string ownerEmail, string ownerName, string placeName, string customerName, string date, string timeSlot, string number);
        Task SendNewReviewNotificationAsync(string ownerEmail, string ownerName, string placeName, string customerName, int rating, string comment);
        Task SendSupportTicketAsync(SupportTicketViewModel model);

    }
}