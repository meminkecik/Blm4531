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
        /// Kullanıcının konumuna en yakın firmaları bulur
        /// 
        /// Bu endpoint, verilen koordinatlara en yakın aktif firmaları
        /// Haversine formülü kullanarak sıralar ve mesafe bilgisi ile döndürür.
        /// 
        /// Hesaplama:
        /// - Haversine formülü ile Great Circle Distance hesaplanır
        /// - Sonuçlar km cinsinden mesafe bilgisi içerir
        /// - Sadece aktif firmalar dahil edilir
        /// - Mesafeye göre artan sırada sıralanır
        /// 
        /// Limit: 1-50 arası firma döndürülebilir.
        /// </summary>
        /// <param name="latitude">Kullanıcının enlem bilgisi (örn: 41.0082)</param>
        /// <param name="longitude">Kullanıcının boylam bilgisi (örn: 29.0094)</param>
        /// <param name="limit">Döndürülecek maksimum firma sayısı (varsayılan: 10)</param>
        /// <returns>En yakın firmalar listesi (mesafe bilgisi ile)</returns>
        /// <response code="200">Firmalar başarıyla döndürüldü</response>
        /// <response code="400">Limit 1-50 arasında olmalıdır</response>
        [HttpGet("nearest")]
        public async Task<ActionResult<List<CompanyDto>>> GetNearestCompanies(
            [FromQuery] double latitude, 
            [FromQuery] double longitude,
            [FromQuery] int limit = 10)
        {
            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit 1-50 arasında olmalıdır.");
            }

            var companies = await _locationService.GetNearestCompaniesAsync(latitude, longitude, limit);
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

        /// <summary>
        /// Firma kullanıcısının kendi profil bilgilerini günceller
        /// 
        /// Bu endpoint, firmanın kendi bilgilerini güncellemesini sağlar.
        /// Güncellenebilecek bilgiler: kişisel bilgiler, firma adı, iletişim,
        /// adres, koordinatlar ve hizmet verilen bölgeler.
        /// 
        /// Özellikler:
        /// - Partial update: Sadece gönderilen alanlar güncellenir
        /// - Adres ID'leri il/ilçe isimlerine çevrilir
        /// - UpdatedAt otomatik güncellenir
        /// - Sadece Company rolü yetki sahibidir
        /// </summary>
        /// <param name="dto">Güncellenecek firma bilgileri</param>
        /// <returns>Güncellenmiş firma bilgileri</returns>
        /// <response code="200">Profil başarıyla güncellendi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">Sadece Company rolü yetkilidir</response>
        /// <response code="404">Firma bulunamadı</response>
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
        /// <summary>
        /// Sistemdeki tüm aktif firmaları listeler
        /// 
        /// Bu endpoint, kayıtlı tüm aktif firmaları döndürür.
        /// Sadece IsActive=true olan firmalar dahil edilir.
        /// 
        /// Kullanım: Admin paneli veya genel firma listesi için.
        /// </summary>
        /// <returns>Tüm aktif firmaların listesi</returns>
        /// <response code="200">Firmalar başarıyla döndürüldü</response>
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
        /// <param name="locationDto">Kullanıcı konum bilgisi</param>
        /// <returns>Başarı mesajı</returns>
        /// <response code="200">Konum bilgisi alındı</response>
        [HttpPost("location")]
        public Task<ActionResult> SaveUserLocation([FromBody] LocationDto locationDto)
        {
            // Kullanıcı konumunu kaydet (opsiyonel)
            // Bu endpoint gelecekte analytics için kullanılabilir
            return Task.FromResult<ActionResult>(Ok(new { message = "Konum bilgisi alındı." }));
        }
    }
}
