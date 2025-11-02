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
    /// <summary>
    /// Ticket Controller - Müşteri iletişim taleplerini yönetir
    /// 
    /// Bu controller, kullanıcıların firmalardan yardım istemesini sağlayan
    /// ticket sistemini yönetir. Ticket'lar iletişim talepleri, şikayetler
    /// veya genel sorular için kullanılabilir.
    /// 
    /// Yetkilendirme:
    /// - Herkes yeni ticket oluşturabilir
    /// - Firmalar sadece kendi ticket'larını görebilir
    /// - Admin tüm ticket'ları görebilir
    /// </summary>
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

        /// <summary>
        /// Yeni ticket oluşturur
        /// 
        /// Kullanıcılar bu endpoint ile firmalardan yardım isteyebilir.
        /// Ticket oluşturulduktan sonra otomatik olarak admin'e email bildirimi gönderilir.
        /// 
        /// Gerekli bilgiler:
        /// - Gönderen kişi bilgileri (isim, email, telefon)
        /// - Konu ve mesaj
        /// - İlgili firma ID'si (opsiyonel)
        /// </summary>
        /// <param name="dto">Ticket oluşturma bilgileri</param>
        /// <returns>Başarı mesajı ve ticket ID</returns>
        /// <response code="200">Ticket başarıyla oluşturuldu</response>
        [HttpPost]
        public async Task<ActionResult> CreateTicket([FromBody] TicketDto dto)
        {
            var ticket = await _ticketService.CreateTicketAsync(dto);
            return Ok(new { message = "Ticket başarıyla oluşturuldu.", ticketId = ticket.Id });
        }

        /// <summary>
        /// Ticket'ları listeler (rol bazlı)
        /// 
        /// Bu endpoint, kullanıcı rolüne göre farklı davranış sergiler:
        /// 
        /// - Admin: Tüm ticket'ları görür (tüm firmalar)
        /// - Company: Sadece kendi ticket'larını görür
        /// 
        /// Sonuçlar oluşturulma tarihine göre azalan sırada gelir.
        /// </summary>
        /// <returns>Ticket listesi</returns>
        /// <response code="200">Ticket listesi başarıyla döndürüldü</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">Bu işlem için yetki gerekli</response>
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

        /// <summary>
        /// Ticket durumunu günceller (rol bazlı)
        /// 
        /// Ticket durumları: Pending, InProgress, Resolved, Closed
        /// 
        /// Yetkilendirme:
        /// - Admin: Herhangi bir ticket'ın durumunu değiştirebilir
        /// - Company: Sadece kendi ticket'larının durumunu değiştirebilir
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <param name="status">Yeni durum</param>
        /// <returns>Başarı mesajı</returns>
        /// <response code="200">Ticket durumu güncellendi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Ticket bulunamadı</response>
        /// <response code="403">Bu işlem için yetki gerekli</response>
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
