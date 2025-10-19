using System.Net;
using System.Net.Mail;
using System.Text;
using Nearest.DTOs;

namespace Nearest.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailDto emailDto)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                using var message = new MailMessage();
                message.From = new MailAddress(smtpUsername, "Nearest Oto Kurtarma");
                message.To.Add(emailDto.To);
                message.Subject = emailDto.Subject;
                message.Body = emailDto.Body;
                message.IsBodyHtml = emailDto.IsHtml;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {emailDto.To}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {emailDto.To}");
                return false;
            }
        }

        public async Task<bool> SendTicketNotificationAsync(TicketDto ticketDto)
        {
            var adminEmail = _configuration["Email:AdminEmail"] ?? "nearestmek@gmail.com";
            
            var subject = $"Yeni Ticket: {ticketDto.Subject}";
            var body = $@"
                <h2>Yeni Ticket Bildirimi</h2>
                <p><strong>Gönderen:</strong> {ticketDto.FirstName} {ticketDto.LastName}</p>
                <p><strong>Email:</strong> {ticketDto.Email}</p>
                <p><strong>Telefon:</strong> {ticketDto.PhoneNumber}</p>
                <p><strong>Konu:</strong> {ticketDto.Subject}</p>
                <p><strong>Mesaj:</strong></p>
                <p>{ticketDto.Message}</p>
                <hr>
                <p><em>Bu mesaj Nearest Oto Kurtarma sistemi tarafından otomatik olarak gönderilmiştir.</em></p>
            ";

            var emailDto = new EmailDto
            {
                To = adminEmail,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(emailDto);
        }
    }
}
