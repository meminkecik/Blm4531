using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;

namespace Nearest.Controllers
{
    /// <summary>
    /// Admin Controller - Sistem yöneticisi işlemleri için endpoint'ler sağlar
    /// 
    /// Bu controller, yöneticilerin sistemi yönetmesi için gerekli işlevleri içerir.
    /// Tüm admin endpoint'leri JWT token gerektirir ve sadece Admin rolüne sahip
    /// kullanıcılar erişebilir.
    /// 
    /// Admin yetkileri:
    /// - Türkiye adres verilerini güncellemek (turkiyeapi.dev API'den)
    /// - Tüm ticket'ları görüntülemek
    /// - Sistemi yönetmek
    /// 
    /// Güvenlik: Tüm endpoint'ler [Authorize] attribute'u ile korunur.
    /// </summary>
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

        /// <summary>
        /// Admin kullanıcısı giriş yapar
        /// 
        /// Giriş işlemi sırasında:
        /// - Email ve şifre doğrulanır
        /// - Aktif admin kullanıcıları giriş yapabilir
        /// - Başarılı giriş sonrası JWT token döndürülür (7 gün geçerli)
        /// 
        /// Token içinde şu bilgiler bulunur:
        /// - Admin ID (AdminId)
        /// - Email ve isim bilgileri
        /// - Rol bilgisi (Role: "Admin")
        /// </summary>
        /// <param name="loginDto">Admin giriş bilgileri (email ve şifre)</param>
        /// <returns>JWT token ve admin bilgileri</returns>
        /// <response code="200">Giriş başarılı, token döndürüldü</response>
        /// <response code="401">Email veya şifre hatalı</response>
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

        /// <summary>
        /// Türkiye adres verilerini günceller
        /// 
        /// Bu endpoint, turkiyeapi.dev API'den en güncel Türkiye adres verilerini çeker
        /// ve veritabanını günceller. Tüm iller, ilçeler ve il-ilçe ilişkileri senkronize edilir.
        /// 
        /// Güncelleme işlemi:
        /// - 81 ili ve tüm ilçeleri çeker
        /// - Yeni kayıtları ekler, mevcut kayıtları günceller
        /// - İl-ilçe ilişkilerini kurar
        /// 
        /// Yetkilendirme: Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.
        /// Bu işlem uzun sürebilir (81 il × ~100+ ilçe).
        /// </summary>
        /// <returns>Güncelleme sonucu mesajı</returns>
        /// <response code="200">Adres verileri başarıyla güncellendi</response>
        /// <response code="403">Bu işlem için admin yetkisi gerekli</response>
        /// <response code="500">API'den veri çekilirken hata oluştu</response>
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

        /// <summary>
        /// Sistemdeki tüm ticket'ları listeler
        /// 
        /// Bu endpoint, tüm firmalar ve adminler tarafından oluşturulan tüm ticket'ları
        /// döndürür. Ticket'lar oluşturulma tarihine göre azalan sırada gelir.
        /// 
        /// Dönen veriler:
        /// - Ticket bilgileri (konu, mesaj, durum)
        /// - İlgili firma bilgileri (varsa)
        /// - Oluşturulma tarihi
        /// - Durum bilgisi
        /// 
        /// Yetkilendirme: Sadece Admin rolüne sahip kullanıcılar bu listeyi görebilir.
        /// </summary>
        /// <returns>Tüm ticket'ların listesi</returns>
        /// <response code="200">Ticket listesi başarıyla döndürüldü</response>
        /// <response code="403">Bu işlem için admin yetkisi gerekli</response>
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
