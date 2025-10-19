using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public TicketService(ApplicationDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.Company)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket> CreateTicketAsync(DTOs.TicketDto ticketDto)
        {
            var ticket = _mapper.Map<Ticket>(ticketDto);
            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Email bildirimi gönder
            await _emailService.SendTicketNotificationAsync(ticketDto);

            return ticket;
        }
    }
}
