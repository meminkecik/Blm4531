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
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITowTruckService _towTruckService;

        public AdminController(
            IAdminService adminService,
            IAddressService addressService,
            ITicketService ticketService,
            ApplicationDbContext context,
            IMapper mapper,
            ITowTruckService towTruckService)
        {
            _adminService = adminService;
            _addressService = addressService;
            _ticketService = ticketService;
            _context = context;
            _mapper = mapper;
            _towTruckService = towTruckService;
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
            var companies = await _context.Companies.ToListAsync();
            return Ok(_mapper.Map<List<CompanyDto>>(companies));
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
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FirstName)) company.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) company.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.CompanyName)) company.CompanyName = dto.CompanyName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) company.PhoneNumber = dto.PhoneNumber;
            if (dto.ProvinceId.HasValue) company.ProvinceId = dto.ProvinceId.Value;
            if (dto.DistrictId.HasValue) company.DistrictId = dto.DistrictId.Value;
            if (!string.IsNullOrEmpty(dto.FullAddress)) company.FullAddress = dto.FullAddress;
            if (dto.Latitude.HasValue) company.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) company.Longitude = dto.Longitude;
            if (!string.IsNullOrEmpty(dto.ServiceCity)) company.ServiceCity = dto.ServiceCity;
            if (!string.IsNullOrEmpty(dto.ServiceDistrict)) company.ServiceDistrict = dto.ServiceDistrict;
            if (!string.IsNullOrEmpty(dto.Email)) company.Email = dto.Email;
            if (dto.IsActive.HasValue) company.IsActive = dto.IsActive.Value;
            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<CompanyDto>(company));
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
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
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
            var towTrucks = await _context.TowTrucks.Include(t => t.Company).ToListAsync();
            return Ok(_mapper.Map<List<TowTruckDto>>(towTrucks));
        }

        /// <summary>
        /// Çekici güncelle (admin)
        /// </summary>
        [HttpPut("towtrucks/{id}")]
        [Authorize]
        public async Task<ActionResult<TowTruckDto>> UpdateTowTruck(int id, [FromBody] Nearest.DTOs.TowTruck.AdminTowTruckUpdateDto dto)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin") return Forbid();
            var towTruck = await _context.TowTrucks.FirstOrDefaultAsync(t => t.Id == id);
            if (towTruck == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.DriverName)) towTruck.DriverName = dto.DriverName;
            if (!string.IsNullOrEmpty(dto.LicensePlate)) towTruck.LicensePlate = dto.LicensePlate;
            if (dto.IsActive.HasValue) towTruck.IsActive = dto.IsActive.Value;
            if (dto.CompanyId.HasValue) towTruck.CompanyId = dto.CompanyId.Value;
            towTruck.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<TowTruckDto>(towTruck));
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
            var towTruck = await _context.TowTrucks.FirstOrDefaultAsync(t => t.Id == id);
            if (towTruck == null) return NotFound();
            _context.TowTrucks.Remove(towTruck);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Çekici başarıyla silindi." });
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
