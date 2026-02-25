using ActPro.Domain;
using ActPro.Domain.Models;
using ActPro.Helpers;
using ActPro.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ActPro.Services.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        //Account Confirmation
        public async Task SendEmailAsync(string email, string userName, string htmlMessage)
        {
            string subject = string.Format(DomainResources.EmailConfirmationSubject, userName);

            string content = $@"
                    <p>{DomainResources.WelcomeToActPro}</p>
                    <div style=""text-align: center; margin: 35px 0;"">
                        <a href=""{htmlMessage}"" style=""background-color: #198754; color: white; padding: 16px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;"">{DomainResources.ConfirmMyAccount}</a>
                    </div>
                    <p style=""font-size: 14px; color: #777;"">{DomainResources.ButtonValid24Hours}<br>{DomainResources.IgnoreEmail}</p>";

            string html = BuildEmailTemplate(DomainResources.EmailConfirmationTitle, content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Made reservation
        public async Task SendBookingConfirmationAsync(string email, string firstName, string placeName, string date, string timeSlot)
        {
            string subject = string.Format(DomainResources.ReservationConfirmationSubject, placeName);

            string content = $@"
             <p>{DomainResources.Greeting}, {firstName}</p>
             <p>{DomainResources.ReservationConfirmedText} {DomainResources.ReservationDetailsText}</p>
             <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #198754; border-radius: 4px; margin: 20px 0;'>
                 <li style='margin-bottom: 10px; font-size: 16px;'><strong>{DomainResources.PlaceLabel}:</strong> {placeName}</li>
                 <li style='margin-bottom: 10px; font-size: 16px;'><strong>{DomainResources.DateLabel}:</strong> {date}</li>
                 <li style='font-size: 16px;'><strong>{DomainResources.TimeLabel}:</strong> {timeSlot}</li>
             </ul>
             <p>{DomainResources.ShowEmailOnArrival}</p>";
            string html = BuildEmailTemplate(DomainResources.ReservationConfirmationTitle, content, DomainResources.MyReservations, "https://actprobg.com/Reservation/MyReservations");
            await ExecuteSendAsync(email, subject, html);
        }

        //Cancel reservation
        public async Task SendBookingCancellationAsync(string email, string firstName, string placeName, string date, string timeSlot)
        {
            string subject = string.Format(DomainResources.ReservationCancelledSubject, placeName);
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{DomainResources.ReservationCanceledText}</p>
                <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #dc3545; border-radius: 4px; margin: 20px 0;'>
                    <li style='margin-bottom: 10px; font-size: 16px;'><strong>{DomainResources.PlaceLabel}:</strong> {placeName}</li>
                    <li style='margin-bottom: 10px; font-size: 16px;'><strong>{DomainResources.DateLabel}:</strong> {date}</li>
                    <li style='font-size: 16px;'><strong>{DomainResources.TimeLabel}:</strong> {timeSlot}</li>
                </ul>
                <p>{DomainResources.BookNewTime}</p>";

            string html = BuildEmailTemplate(DomainResources.ReservationCancelledTitle, content, DomainResources.NewReservationBtn, "https://actprobg.com/");
            await ExecuteSendAsync(email, subject, html);
        }

        //Password reset
        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            string subject = DomainResources.PasswordResetSubject;
            string content = $@"
                <p>{DomainResources.Greeting},</p>
                <p>{DomainResources.PasswordResetRequestText}</p>
                <p style='font-size: 13px; color: #6c757d; margin-top: 20px;'>{DomainResources.IgnorePasswordReset}</p>";

            string html = BuildEmailTemplate(DomainResources.PasswordResetTitle, content, DomainResources.ChangePasswordBtn, resetLink);
            await ExecuteSendAsync(email, subject, html);
        }

        // Password Changed Notification
        public async Task SendPasswordChangedNotificationAsync(string email, string firstName)
        {
            string subject = DomainResources.PasswordChangedSubject;
            string content = $@"
            <p>{DomainResources.Greeting}, {firstName}</p>
            <p>{DomainResources.PasswordChangedConfirmText}</p>
            <div style='padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107; margin: 20px 0;'>
                <p style='margin: 0; font-size: 14px; color: #856404;'>
                    <strong>{DomainResources.ImportantSecurityWarning}</strong>
                </p>
            </div>
            <p>{DomainResources.IgnoreIfNoChange}</p>";

            string html = BuildEmailTemplate(DomainResources.PasswordChangedTitle, content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Delete profile
        public async Task SendProfileDeletedAsync(string email, string firstName)
        {
            string subject = DomainResources.ProfileDeletedSubject;
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{DomainResources.ProfileDeletedText}</p>
                <p>{DomainResources.SorryToSeeYouGo}</p>";

            string html = BuildEmailTemplate(DomainResources.ProfileDeletedTitle, content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Approved place
        public async Task SendPlaceApprovedAsync(string email, string firstName, string placeName)
        {
            string subject = DomainResources.PlaceApprovedSubject;
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{DomainResources.PlaceApprovedText}</p>
                <p>{DomainResources.PlaceCanBeBooked}</p>";

            string html = BuildEmailTemplate(DomainResources.PlaceApprovedTitle, content, DomainResources.ManagePlaceBtn, "https://actprobg.com/Owner/Dashboard");
            await ExecuteSendAsync(email, subject, html);
        }

        //Rejected place
        public async Task SendPlaceRejectedAsync(string email, string firstName, string placeName)
        {
            string subject = DomainResources.PlaceRejectedSubject;
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{string.Format(DomainResources.PlaceRejectedText, placeName)}</p>             
                <p>{DomainResources.MakeCorrectionsAndRetry}</p>";

            string html = BuildEmailTemplate(DomainResources.PlaceRejectedTitle, content, DomainResources.TryAgainBtn, "https://actprobg.com/Owner/Index");
            await ExecuteSendAsync(email, subject, html);
        }

        //Deleted place
        public async Task SendPlaceDeletedAsync(string email, string firstName, string placeName)
        {
            string subject = DomainResources.PlaceDeletedSubject;
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{string.Format(DomainResources.PlaceDeletedByAdminText, placeName)}</p>
                <p>{DomainResources.ContactSupportIfMistake}</p>";

            string html = BuildEmailTemplate(DomainResources.PlaceDeletedTitle, content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Change reservation time
        public async Task SendReservationTimeChangedAsync(string email, string firstName, string placeName, string oldTime, string newTime, string date)
        {
            string subject = DomainResources.ReservationTimeChangedSubject;
            string content = $@"
                <p>{DomainResources.Greeting}, {firstName}</p>
                <p>{DomainResources.ReservationTimeChangedText}</p>
                <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #ffc107; border-radius: 4px; margin: 20px 0;'>
                    <li style='margin-bottom: 10px;'><strong>{DomainResources.DateLabel}:</strong> {date}</li>
                    <li style='margin-bottom: 10px;'><strong>{DomainResources.OldTimeLabel}:</strong> <span style='text-decoration: line-through; color: #dc3545;'>{oldTime}</span></li>
                    <li><strong>{DomainResources.NewTimeLabel}:</strong> <span style='color: #198754; font-weight: bold; font-size: 16px;'>{newTime}</span></li>
                </ul>";

            string html = BuildEmailTemplate(DomainResources.ReservationTimeChangedTitle, content, DomainResources.MyReservations, "https://actprobg.com/Reservation/MyReservations");
            await ExecuteSendAsync(email, subject, html);
        }

        //Support ticket
        public async Task SendSupportTicketAsync(SupportTicketViewModel model)
        {
            string adminSubject = string.Format(DomainResources.NewSupportTicketSubject, model.Subject, model.FullName);
            string adminHtml = $@"
            <h2>{DomainResources.NewSupportTicketTitle}</h2>
            <p><strong>{DomainResources.FromLabel}:</strong> {model.FullName} ({model.Email})</p>
            <p><strong>{DomainResources.IssueTypeLabel}:</strong> {model.Subject}</p>
            <p><strong>{DomainResources.DescriptionLabel}:</strong></p>
            <p style='background: #f8f9fa; pading: 15px;'>{model.Description}</p>";

            await ExecuteSendAsync("contact@actprobg.com", adminSubject, adminHtml);

            string userSubject = DomainResources.SupportTicketReceivedSubject;
            string userContent = $@"
            <p>{DomainResources.Greeting}, {model.FullName},</p>
            <p>{DomainResources.ThankYouForContacting} <strong>'{model.Subject}'</strong> {DomainResources.TicketReceivedSuccessfully}.</p>
            <p>{DomainResources.TeamWillContact}</p>";

            string userHtml = BuildEmailTemplate(DomainResources.SupportTicketReceivedTitle, userContent, DomainResources.GoToSiteBtn, "https://actprobg.com");
            await ExecuteSendAsync(model.Email, userSubject, userHtml);
        }

        // Send notification to owner for new booking
        public async Task SendNewBookingNotificationAsync(string ownerEmail, string ownerName, string placeName, string customerName, string date, string timeSlot, string number)
        {
            string subject = string.Format(DomainResources.NewReservationSubject, placeName);

            string content = $@"
            <p>{DomainResources.Greeting}, {ownerName},</p>
            <p>{DomainResources.YouHaveNewBooking} <strong>{placeName}</strong>.</p>
            <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #198754; border-radius: 4px; margin: 20px 0;'>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.CustomerLabel}:</strong> {customerName}</li>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.NumberLabel}:</strong> {number}</li>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.DateLabel}:</strong> {date}</li>
                <li style='font-size: 16px;'><strong>{DomainResources.TimeLabel}:</strong> {timeSlot}</li>
            </ul>
            <p>{DomainResources.ManageReservationsFromPanel}</p>";

            string html = BuildEmailTemplate(DomainResources.NewBookingTitle, content, DomainResources.GoToDashboardBtn, "https://actprobg.com/Owner/Dashboard/Index");
            await ExecuteSendAsync(ownerEmail, subject, html);
        }

        // Send notification to owner when a customer cancels a booking
        public async Task SendBookingCancellationToOwnerAsync(string ownerEmail, string ownerName, string placeName, string customerName, string date, string timeSlot, string number)
        {
            string subject = string.Format(DomainResources.ReservationCancelledByOwnerSubject, placeName);

            string content = $@"
            <p>{DomainResources.Greeting}, {ownerName},</p>
            <p>{DomainResources.CustomerCancelledBooking} <strong>{placeName}</strong>.</p>
            <p>{DomainResources.CancelledBookingDetails}</p>
            <ul style='list-style-type: none; padding: 15px; background-color: #fff5f5; border-left: 4px solid #dc3545; border-radius: 4px; margin: 20px 0;'>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.CustomerLabel}:</strong> {customerName}</li>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.NumberLabel}:</strong> {number}</li>
                <li style='margin-bottom: 10px;'><strong>{DomainResources.DateLabel}:</strong> {date}</li>
                <li style='font-size: 16px;'><strong>{DomainResources.TimeLabel}:</strong> {timeSlot}</li>
            </ul>
            <p>{DomainResources.TimeNowAvailable}</p>";

            string html = BuildEmailTemplate(DomainResources.BookingCancelledByCustomerTitle, content, DomainResources.GoToDashboardBtn, "https://actprobg.com/Owner/Dashboard/Index");
            await ExecuteSendAsync(ownerEmail, subject, html);
        }

        // Send notification to owner for new review
        public async Task SendNewReviewNotificationAsync(string ownerEmail, string ownerName, string placeName, string customerName, int rating, string comment)
        {
            string subject = string.Format(DomainResources.NewReviewSubject, placeName);

            string stars = new string('⭐', rating);

            string content = $@"
            <p>{DomainResources.Greeting}, {ownerName},</p>
            <p>{DomainResources.UserLeftComment} <strong>{placeName}</strong>.</p>
            <div style='padding: 20px; background-color: #fffaf0; border: 1px dashed #ffc107; border-radius: 8px; margin: 20px 0;'>
                <p style='margin: 0; font-size: 18px;'>{stars} ({rating}/5)</p>
                <p style='font-style: italic; margin-top: 10px; color: #555;'>""{comment}""</p>
            </div>
            <p>{DomainResources.FeedbackImportantForBusiness}</p>";

            string html = BuildEmailTemplate(DomainResources.NewReviewTitle, content, DomainResources.GoToDashboardBtn, "https://actprobg.com/Owner/Dashboard/Index");
            await ExecuteSendAsync(ownerEmail, subject, html);
        }

        //Email template builder
        private string BuildEmailTemplate(string headerText, string bodyContent, string buttonText = null, string buttonUrl = null)
        {
            string buttonHtml = "";
            if (!string.IsNullOrEmpty(buttonText) && !string.IsNullOrEmpty(buttonUrl))
            {
                buttonHtml = $@"
                <table role='presentation' border='0' cellpadding='0' cellspacing='0' style='margin: 30px auto;'>
                    <tbody>
                        <tr>
                            <td align='center' bgcolor='#198754' style='border-radius: 6px;'>
                                <a href='{buttonUrl}' target='_blank' style='font-size: 16px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; padding: 14px 28px; border-radius: 6px; border: 1px solid #198754; display: inline-block; font-weight: bold;'>{buttonText}</a>
                            </td>
                        </tr>
                    </tbody>
                </table>";
            }

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='margin: 0; padding: 0; background-color: #e9ecef; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
                <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #ffffff; padding: 40px 0;'>
                    <tr>
                        <td align='center'>
                            <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #fcfcfc; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); overflow: hidden;'>
                                
                                <tr>
                                    <td style='background-color: #198754; padding: 35px 40px; text-align: center;'>
                                        <h1 style='margin: 0; color: #ffffff; font-size: 28px; letter-spacing: 1px; font-weight: 800;'>ActPro</h1>
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding: 40px; color: #333333; font-size: 16px; line-height: 1.6;'>
                                        <h2 style='margin-top: 0; margin-bottom: 25px; font-size: 22px; color: #198754;'>{headerText}</h2>
                                        {bodyContent}
                                        {buttonHtml}
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding: 25px 40px; background-color: #f1f3f5; border-top: 1px solid #dee2e6; text-align: center;'>
                                        <p style='margin: 0 0 8px 0; color: #6c757d; font-size: 13px;'>{DomainResources.AutoMessageDisclaimer}</p>
                                        <p style='margin: 0; color: #adb5bd; font-size: 12px;'>&copy; {DateTime.Now.Year} ActPro. {DomainResources.AllRightsReserved}</p>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        private async Task ExecuteSendAsync(string email, string subject, string finalHtml)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = finalHtml };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Timeout = 5000;
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---> EMAIL ERROR: {ex.Message}");
            }
        }
    }
}