using Nearest.DTOs;

namespace Nearest.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailDto emailDto);
        Task<bool> SendTicketNotificationAsync(TicketDto ticketDto);
    }
}
