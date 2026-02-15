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
            string subject = $"Потвърждение на акаут: {userName}";

            string content = $@"
                    <p>Радваме се, че избрахте <strong>ActPro</strong>. Остават само няколко стъпки, преди да можете да резервирате любимите си спортни зали.</p>
                    <div style=""text-align: center; margin: 35px 0;"">
                        <a href=""{htmlMessage}"" style=""background-color: #198754; color: white; padding: 16px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;"">ПОТВЪРДИ МОЯ АКАУНТ</a>
                    </div>
                    <p style=""font-size: 14px; color: #777;"">Бутонът е валиден за следващите 24 часа.<br>Ако не сте правили регистрация при нас, моля игнорирайте този имейл.</p>";

            string html = BuildEmailTemplate("Потвърждение на акаут", content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Made reservation
        public async Task SendBookingConfirmationAsync(string email, string firstName, string placeName, string date, string timeSlot)
        {
            string subject = $"Потвърждение на резервация: {placeName}";

            string content = $@"
             <p>Здравейте, {firstName}</p>
             <p>Вашата резервация беше успешно потвърдена. Детайли за посещението:</p>
             <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #198754; border-radius: 4px; margin: 20px 0;'>
                 <li style='margin-bottom: 10px; font-size: 16px;'><strong>Обект:</strong> {placeName}</li>
                 <li style='margin-bottom: 10px; font-size: 16px;'><strong>Дата:</strong> {date}</li>
                 <li style='font-size: 16px;'><strong>Час:</strong> {timeSlot}</li>
             </ul>
             <p>Моля, представете този имейл при пристигане на мястото.</p>";
            string html = BuildEmailTemplate("Успешна резервация", content, "Към моите резервации", "https://actprobg.com/Reservation/MyReservations");
            await ExecuteSendAsync(email, subject, html);
        }

        //Cancel reservation
        public async Task SendBookingCancellationAsync(string email, string firstName, string placeName, string date, string timeSlot)
        {
            string subject = $"Отказана резервация: {placeName}";
            string content = $@"
                <p>Здравейте, {firstName}</p>
                <p>Уведомяваме Ви, че следната резервация беше <strong>отказана</strong>:</p>
                <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #dc3545; border-radius: 4px; margin: 20px 0;'>
                    <li style='margin-bottom: 10px; font-size: 16px;'><strong>Обект:</strong> {placeName}</li>
                    <li style='margin-bottom: 10px; font-size: 16px;'><strong>Дата:</strong> {date}</li>
                    <li style='font-size: 16px;'><strong>Час:</strong> {timeSlot}</li>
                </ul>
                <p>Ако желаете да запазите нов час, можете да го направите през нашата платформа.</p>";

            string html = BuildEmailTemplate("Отказана резервация", content, "Нова резервация", "https://actprobg.com/");
            await ExecuteSendAsync(email, subject, html);
        }

        //Password reset
        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            string subject = "Възстановяване на парола";
            string content = $@"
                <p>Здравейте,</p>
                <p>Получихме заявка за възстановяване на паролата за Вашия профил. Кликнете на бутона по-долу, за да зададете нова.</p>
                <p style='font-size: 13px; color: #6c757d; margin-top: 20px;'>Ако не сте заявявали промяна, моля, игнорирайте този имейл.</p>";

            string html = BuildEmailTemplate("Възстановяване на парола", content, "Смяна на паролата", resetLink);
            await ExecuteSendAsync(email, subject, html);
        }

        //Delete profile
        public async Task SendProfileDeletedAsync(string email, string firstName)
        {
            string subject = "Потвърждение за изтриване на профил";
            string content = $@"
                <p>Здравейте, {firstName}</p>
                <p>Вашият профил в ActPro беше успешно изтрит от нашата система, заедно с всички свързани с него лични данни и история.</p>
                <p>Съжаляваме, че си тръгвате. Винаги сте добре дошли отново!</p>";

            string html = BuildEmailTemplate("Изтрит профил", content);
            await ExecuteSendAsync(email, subject, html);
        }

        //Approved place
        public async Task SendPlaceApprovedAsync(string email, string firstName, string placeName)
        {
            string subject = "Вашият обект е одобрен";
            string content = $@"
                <p>Здравейте, {firstName}</p>
                <p>Имаме удоволствието да Ви съобщим, че вашият обект <strong>{placeName}</strong> премина успешна проверка от нашия екип и вече е активен в платформата.</p>
                <p>Потребителите вече могат да разглеждат обекта и да правят резервации при вас.</p>";

            string html = BuildEmailTemplate("Обектът е активен", content, "Управление на обекта", "https://actprobg.com/Owner/Dashboard");
            await ExecuteSendAsync(email, subject, html);
        }

        //Rejected place
        public async Task SendPlaceRejectedAsync(string email, string firstName, string placeName)
        {
            string subject = "Статус на заявката за обект";
            string content = $@"
                <p>Здравейте, {firstName}</p>
                <p>След преглед на Вашия обект <strong>{placeName}</strong>, установихме, че той не отговаря на изискванията на платформата в настоящия си вид.</p>             
                <p>Моля, направете необходимите корекции и подайте заявката отново.</p>";

            string html = BuildEmailTemplate("Отказ за публикуване", content, "Към профила", "https://actprobg.com/Owner/Index");
            await ExecuteSendAsync(email, subject, html);
        }

        //Change reservation time
        public async Task SendReservationTimeChangedAsync(string email, string firstName, string placeName, string oldTime, string newTime, string date)
        {
            string subject = "Промяна в часа на резервация";
            string content = $@"
                <p>Здравейте, {firstName}</p>
                <p>Администратор промени часа на Вашата резервация за <strong>{placeName}</strong>.</p>
                <ul style='list-style-type: none; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #ffc107; border-radius: 4px; margin: 20px 0;'>
                    <li style='margin-bottom: 10px;'><strong>Дата:</strong> {date}</li>
                    <li style='margin-bottom: 10px;'><strong>Стар час:</strong> <span style='text-decoration: line-through; color: #dc3545;'>{oldTime}</span></li>
                    <li><strong>Нов час:</strong> <span style='color: #198754; font-weight: bold; font-size: 16px;'>{newTime}</span></li>
                </ul>
                <p>Ако новият час не Ви е удобен, можете да анулирате резервацията от профила си.</p>";

            string html = BuildEmailTemplate("Променен час", content, "Към резервациите", "https://actprobg.com/Reservation/MyReservations");
            await ExecuteSendAsync(email, subject, html);
        }

        //Deleted place
        public async Task SendPlaceDeletedAsync(string email, string firstName, string placeName)
        {
            string subject = "Вашият обект е премахнат";
            string content = $@"
                <p>Уважаеми/а {firstName}</p>
                <p>Информираме Ви, че Вашият обект <strong>{placeName}</strong> беше премахнат от нашата платформа от администратор.</p>
                <p>Ако смятате, че е станала грешка, моля свържете се с поддръжката ни.</p>";

            string html = BuildEmailTemplate("Премахнат обект", content);
            await ExecuteSendAsync(email, subject, html);
        }
        //Support ticket
        public async Task SendSupportTicketAsync(SupportTicketViewModel model)
        {
            string adminSubject = $"[Support Ticket] {model.Subject} - от {model.FullName}";
            string adminHtml = $@"
            <h2>Нова заявка за поддръжка</h2>
            <p><strong>От:</strong> {model.FullName} ({model.Email})</p>
            <p><strong>Тип проблем:</strong> {model.Subject}</p>
            <p><strong>Описание:</strong></p>
            <p style='background: #f8f9fa; pading: 15px;'>{model.Description}</p>";

            await ExecuteSendAsync("contact@actprobg.com", adminSubject, adminHtml);

            string userSubject = "ActPro - Вашата заявка е получена";
            string userContent = $@"
            <p>Здравейте, {model.FullName},</p>
            <p>Благодарим Ви, че се свързахте с нас! Вашата заявка относно <strong>'{model.Subject}'</strong> е приета успешно.</p>
            <p>Нашият екип ще прегледа информацията и ще се свърже с вас в рамките на 24 часа.</p>
            <p style='font-size: 12px; color: #666;'>Това е автоматично съобщение, моля не отговаряйте на него.</p>";

            string userHtml = BuildEmailTemplate("Заявка за поддръжка", userContent, "Към сайта", "https://actprobg.com");
            await ExecuteSendAsync(model.Email, userSubject, userHtml);
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
                                        <p style='margin: 0 0 8px 0; color: #6c757d; font-size: 13px;'>Това съобщение е генерирано автоматично. Моля, не отговаряйте на този имейл.</p>
                                        <p style='margin: 0; color: #adb5bd; font-size: 12px;'>&copy; {DateTime.Now.Year} ActPro. Всички права запазени.</p>
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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = finalHtml };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}