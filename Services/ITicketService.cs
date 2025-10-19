using Nearest.Models;

namespace Nearest.Services
{
    public interface ITicketService
    {
        Task<List<Ticket>> GetAllTicketsAsync();
        Task<Ticket> CreateTicketAsync(DTOs.TicketDto ticketDto);
    }
}
