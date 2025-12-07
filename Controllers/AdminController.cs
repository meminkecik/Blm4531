using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;

namespace Nearest.Controllers
{
    /// Admin yetkileri:
    /// - Türkiye adres verilerini güncellemek (turkiyeapi.dev API'den)
    /// - Tüm ticket'ları görüntülemek
    /// - Sistemi yönetmek
    /// 
    /// Güvenlik: Tüm endpoint'ler [Authorize] attribute'u ile korunur.
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAddressService _addressService;
        private readonly ITicketService _ticketService;

        public AdminController(IAdminService adminService, IAddressService addressService, ITicketService ticketService)
        {
            _adminService = adminService;
            _addressService = addressService;
            _ticketService = ticketService;
        }

        /// Token içinde şu bilgiler bulunur:
        /// - Admin ID (AdminId)
        /// - Email ve isim bilgileri
        /// - Rol bilgisi (Role: "Admin")
        [HttpPost("login")]
        public async Task<ActionResult<AdminAuthResponseDto>> Login([FromBody] AdminLoginDto loginDto)
        {
            var result = await _adminService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Email veya şifre hatalı.");
            }

            return Ok(result);
        }

        /// Güncelleme işlemi:
        /// - 81 ili ve tüm ilçeleri çeker
        /// - Yeni kayıtları ekler, mevcut kayıtları günceller
        /// - İl-ilçe ilişkilerini kurar
        /// Yetkilendirme: Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.
        /// Bu işlem uzun sürebilir (81 il × ~100+ ilçe).
        [HttpPut("address")]
        [Authorize]
        public async Task<ActionResult<string>> UpdateAddress()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin")
            {
                return Forbid("Bu işlem için admin yetkisi gerekli.");
            }

            var result = await _addressService.UpdateAddressAsync();
            return Ok(new { message = result });
        }


        /// Dönen veriler:
        /// - Ticket bilgileri (konu, mesaj, durum)
        /// - İlgili firma bilgileri (varsa)
        /// - Oluşturulma tarihi
        /// - Durum bilgisi
        [HttpGet("tickets")]
        [Authorize]
        public async Task<ActionResult<List<Ticket>>> GetAllTickets()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin")
            {
                return Forbid("Bu işlem için admin yetkisi gerekli.");
            }

            var tickets = await _ticketService.GetAllTicketsAsync();
            return Ok(tickets);
        }
    }
}
