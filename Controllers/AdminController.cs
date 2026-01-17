using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.DTOs.TowTruck;
using Nearest.Models;
using Nearest.Services;

namespace Nearest.Controllers
{
    /// <summary>
    /// Admin Controller
    /// 
    /// Admin yetkileri:
    /// - Türkiye adres verilerini güncellemek (turkiyeapi.dev API'den)
    /// - Tüm ticket'ları görüntülemek
    /// - Firma ve çekici yönetimi
    /// - Sistemi yönetmek
    /// 
    /// SOLID: Controller sadece HTTP isteklerini karşılar, iş mantığı Service katmanında.
    /// Güvenlik: Tüm endpoint'ler [Authorize] attribute'u ile korunur.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAddressService _addressService;
        private readonly ITicketService _ticketService;
        private readonly ICompanyService _companyService;
        private readonly ITowTruckService _towTruckService;

        public AdminController(
            IAdminService adminService,
            IAddressService addressService,
            ITicketService ticketService,
            ICompanyService companyService,
            ITowTruckService towTruckService)
        {
            _adminService = adminService;
            _addressService = addressService;
            _ticketService = ticketService;
            _companyService = companyService;
            _towTruckService = towTruckService;
        }

        /// <summary>
        /// Admin girişi yapar
        /// 
        /// Token içinde şu bilgiler bulunur:
        /// - Admin ID (AdminId)
        /// - Email ve isim bilgileri
        /// - Rol bilgisi (Role: "Admin")
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AdminAuthResponseDto>> Login([FromBody] AdminLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _adminService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Email veya şifre hatalı." });
            }

            return Ok(result);
        }

        // --- COMPANY CRUD ---

        /// <summary>
        /// Tüm şirketleri (firmaları) listeler (admin)
        /// </summary>
        [HttpGet("companies")]
        [Authorize]
        public async Task<ActionResult<List<CompanyDto>>> GetAllCompanies()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>
        /// Şirket (firma) güncelle (admin)
        /// </summary>
        [HttpPut("companies/{id}")]
        [Authorize]
        public async Task<ActionResult<CompanyDto>> UpdateCompany(int id, [FromBody] AdminCompanyUpdateDto dto)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var result = await _companyService.UpdateCompanyAsync(id, dto);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Şirket (firma) sil (admin)
        /// </summary>
        [HttpDelete("companies/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var result = await _companyService.DeleteCompanyAsync(id);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Şirket başarıyla silindi." });
        }

        // --- TOWTRUCK CRUD ---

        /// <summary>
        /// Tüm çekicileri listeler (admin)
        /// </summary>
        [HttpGet("towtrucks")]
        [Authorize]
        public async Task<ActionResult<List<TowTruckDto>>> GetAllTowTrucks()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var towTrucks = await _towTruckService.GetAllTowTrucksForAdminAsync();
            return Ok(towTrucks);
        }

        /// <summary>
        /// Çekici güncelle (admin)
        /// </summary>
        [HttpPut("towtrucks/{id}")]
        [Authorize]
        public async Task<ActionResult<TowTruckDto>> UpdateTowTruck(int id, [FromBody] AdminTowTruckUpdateDto dto)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var result = await _towTruckService.UpdateTowTruckByAdminAsync(id, dto);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Çekici sil (admin)
        /// </summary>
        [HttpDelete("towtrucks/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTowTruck(int id)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();

            var result = await _towTruckService.DeleteTowTruckByAdminAsync(id);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Çekici başarıyla silindi." });
        }

        // --- ADDRESS UPDATE ---

        /// <summary>
        /// Türkiye adres verilerini günceller
        /// 
        /// Güncelleme işlemi:
        /// - 81 ili ve tüm ilçeleri çeker
        /// - Yeni kayıtları ekler, mevcut kayıtları günceller
        /// - İl-ilçe ilişkilerini kurar
        /// 
        /// Yetkilendirme: Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.
        /// Bu işlem uzun sürebilir (81 il × ~100+ ilçe).
        /// </summary>
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

        // --- TICKETS ---

        /// <summary>
        /// Tüm ticket'ları listeler
        /// 
        /// Dönen veriler:
        /// - Ticket bilgileri (konu, mesaj, durum)
        /// - İlgili firma bilgileri (varsa)
        /// - Oluşturulma tarihi
        /// - Durum bilgisi
        /// </summary>
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
