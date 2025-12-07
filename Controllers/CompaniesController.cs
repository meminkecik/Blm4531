using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Services;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using AutoMapper;

namespace Nearest.Controllers
{
    /// <summary>
    /// Firmalar Controller - Firma bilgileri ve lokasyon tabanlı arama işlemlerini yönetir
    /// 
    /// Bu controller, oto kurtarma firmaları ile ilgili tüm işlemleri içerir:
    /// - En yakın firmaları bulma (konum tabanlı arama)
    /// - Tüm aktif firmaları listeleme
    /// - Firma profil güncelleme
    /// 
    /// Lokasyon tabanlı arama Haversine formülü kullanılarak gerçekleştirilir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAddressService _addressService;

        public CompaniesController(ILocationService locationService, ApplicationDbContext context, IMapper mapper, IAddressService addressService)
        {
            _locationService = locationService;
            _context = context;
            _mapper = mapper;
            _addressService = addressService;
        }

        /// <summary>
        /// Kullanıcının konumuna göre en yakın firmaları bulur
        /// 
        /// Bu endpoint şu şekilde çalışır:
        /// 1. Kullanıcının konum bilgisinden (latitude/longitude) il ve ilçe bilgisi tespit edilir.
        /// 2. Öncelikle kullanıcının bulunduğu ilçedeki firmalar listelenir.
        /// 3. Eğer ilçede firma yoksa, en yakın ilçedeki firmalar listelenir.
        /// 4. Eğer ilde firma yoksa, ildeki tüm firmalar listelenir.
        /// 5. Eğer hiç firma bulunamazsa, rastgele firmalar listelenir.
        /// 6. Ardından gönderilen koordinatlar (latitude/longitude) üzerinden Haversine
        ///    formülü ile mesafe hesaplar ve en yakın firmalardan limit kadarını döndürür.
        /// 
        /// Limit: 1-50 arası firma döndürülebilir.
        /// </summary>
        [HttpGet("nearest")]
        public async Task<ActionResult<List<CompanyDto>>> GetNearestCompanies(
            [FromQuery] double latitude, 
            [FromQuery] double longitude,
            [FromQuery] int limit = 10,
            [FromQuery] int? provinceId = null,
            [FromQuery] int? districtId = null)
        {
            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit 1-50 arasında olmalıdır.");
            }

            // If provinceId and districtId are not provided, try to determine them from coordinates
            if (!provinceId.HasValue || !districtId.HasValue)
            {
                var locationInfo = await _locationService.GetProvinceAndDistrictFromCoordinatesAsync(latitude, longitude);
                provinceId = locationInfo.provinceId;
                districtId = locationInfo.districtId;
            }

            var companies = await _locationService.GetNearestCompaniesAsync(
                latitude,
                longitude,
                limit,
                provinceId,
                districtId);
            return Ok(companies);
        }

        public class CompanyUpdateDto
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? CompanyName { get; set; }
            public string? PhoneNumber { get; set; }
            public int? ProvinceId { get; set; }
            public int? DistrictId { get; set; }
            public string? FullAddress { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string? ServiceCity { get; set; }
            public string? ServiceDistrict { get; set; }
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<CompanyDto>> UpdateMyProfile([FromBody] CompanyUpdateDto dto)
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Company") return Forbid();
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                return Unauthorized();

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId && c.IsActive);
            if (company == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FirstName)) company.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) company.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.CompanyName)) company.CompanyName = dto.CompanyName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) company.PhoneNumber = dto.PhoneNumber;
            if (dto.ProvinceId.HasValue)
            {
                var cityName = await _addressService.GetCityNameAsync(dto.ProvinceId.Value);
                if (!string.IsNullOrEmpty(cityName))
                {
                    company.ProvinceId = dto.ProvinceId.Value;
                    company.City = cityName;
                }
            }
            if (dto.DistrictId.HasValue)
            {
                var districtName = await _addressService.GetDistrictNameAsync(dto.DistrictId.Value);
                if (!string.IsNullOrEmpty(districtName))
                {
                    company.DistrictId = dto.DistrictId.Value;
                    company.District = districtName;
                }
            }
            if (!string.IsNullOrEmpty(dto.FullAddress)) company.FullAddress = dto.FullAddress;
            if (dto.Latitude.HasValue) company.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) company.Longitude = dto.Longitude;
            if (!string.IsNullOrEmpty(dto.ServiceCity)) company.ServiceCity = dto.ServiceCity;
            if (!string.IsNullOrEmpty(dto.ServiceDistrict)) company.ServiceDistrict = dto.ServiceDistrict;

            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<CompanyDto>(company));
        }

        [HttpGet]
        public async Task<ActionResult<List<CompanyDto>>> GetAllCompanies()
        {
            var companies = await _locationService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>
        /// Kullanıcı konum bilgisini kaydeder
        /// 
        /// Bu endpoint gelecekte analytics ve kullanıcı davranışı
        /// analizi için tasarlanmıştır. Şu an sadece konum alır.
        /// </summary>
        [HttpPost("location")]
        public Task<ActionResult> SaveUserLocation([FromBody] LocationDto locationDto)
        {
            // Kullanıcı konumunu kaydet (opsiyonel)
            // Bu endpoint gelecekte analytics için kullanılabilir
            return Task.FromResult<ActionResult>(Ok(new { message = "Konum bilgisi alındı." }));
        }
    }
}
