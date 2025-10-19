using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;
using System.Security.Claims;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ApplicationDbContext _context;

        public TicketsController(ITicketService ticketService, ApplicationDbContext context)
        {
            _ticketService = ticketService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateTicket([FromBody] TicketDto dto)
        {
            var ticket = await _ticketService.CreateTicketAsync(dto);
            return Ok(new { message = "Ticket başarıyla oluşturuldu.", ticketId = ticket.Id });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Ticket>>> GetTickets()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role == "Admin")
            {
                // Admin tüm ticket'ları görebilir
                var tickets = await _ticketService.GetAllTicketsAsync();
                return Ok(tickets);
            }
            else if (role == "Company")
            {
                // Company sadece kendi ticket'larını görebilir
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return Unauthorized();
                }

                var tickets = await _context.Tickets
                    .Where(t => t.CompanyId == companyId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(tickets);
            }

            return Forbid("Bu işlem için yetki gerekli.");
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<ActionResult> UpdateTicketStatus(int id, [FromBody] TicketStatus status)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role == "Admin")
            {
                // Admin herhangi bir ticket'ın durumunu değiştirebilir
                var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
                if (ticket == null)
                {
                    return NotFound();
                }
                ticket.Status = status;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Ticket durumu güncellendi." });
            }
            else if (role == "Company")
            {
                // Company sadece kendi ticket'larının durumunu değiştirebilir
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return Unauthorized();
                }

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);

                if (ticket == null)
                {
                    return NotFound();
                }

                ticket.Status = status;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Ticket durumu güncellendi." });
            }

            return Forbid("Bu işlem için yetki gerekli.");
        }
    }
}
