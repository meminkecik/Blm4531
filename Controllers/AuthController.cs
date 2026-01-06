using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;
using System.Security.Cryptography;
using System.Text;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }


        /// Kayıt için gerekli bilgiler:
        /// - Firma ve temsilci kişisel bilgileri
        /// - Firma adresi ve konum bilgileri (latitude, longitude)
        /// - Hizmet verilen bölgeler
        /// - İletişim bilgileri (email, telefon)
        /// - KVKK açık rıza onayı (zorunlu)
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(CompanyRegistrationDto dto)
        {
            // KVKK onayı kontrolü
            if (!dto.KvkkConsent)
            {
                return BadRequest("KVKK açık rıza onayı zorunludur.");
            }

            // Email zaten var mı kontrol et
            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            // Telefon numarası zaten var mı kontrol et (unique)
            if (await _context.Companies.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            {
                return BadRequest("Bu telefon numarası zaten kullanılıyor.");
            }

            var company = _mapper.Map<Company>(dto);

            // Province/District isimlerini entegre adres sisteminden resolve et
            if (dto.ProvinceId > 0)
            {
                var cityName = await HttpContext.RequestServices.GetRequiredService<IAddressService>().GetCityNameAsync(dto.ProvinceId);
                if (!string.IsNullOrEmpty(cityName))
                {
                    company.City = cityName;
                    company.ProvinceId = dto.ProvinceId;
                }
            }
            if (dto.DistrictId > 0)
            {
                var districtName = await HttpContext.RequestServices.GetRequiredService<IAddressService>().GetDistrictNameAsync(dto.DistrictId);
                if (!string.IsNullOrEmpty(districtName))
                {
                    company.District = districtName;
                    company.DistrictId = dto.DistrictId;
                }
            }

            company.PasswordHash = HashPassword(dto.Password);

            // KVKK onay bilgilerini kaydet
            company.KvkkConsent = true;
            company.KvkkConsentDate = DateTime.UtcNow;
            company.KvkkConsentVersion = dto.KvkkConsentVersion ?? "1.0";
            company.KvkkConsentIpAddress = GetClientIpAddress();

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }

        /// <summary>
        /// İstemci IP adresini alır
        /// </summary>
        private string GetClientIpAddress()
        {
            // X-Forwarded-For header'ı varsa (proxy/load balancer arkasında)
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            // X-Real-IP header'ı varsa
            var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Doğrudan bağlantı IP'si
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }


        /// Token içinde şu bilgiler bulunur:
        /// - Firma ID (CompanyId)
        /// - Email ve firma adı
        /// - Rol bilgisi (Role: "Company")
        /// </summary>
        /// <param name="dto">Giriş bilgileri (email ve şifre)</param>
        /// <returns>JWT token ve firma bilgileri</returns>
        /// <response code="200">Giriş başarılı, token döndürüldü</response>
        /// <response code="401">Email veya şifre hatalı</response>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(CompanyLoginDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.Email && c.IsActive);

            if (company == null || !VerifyPassword(dto.Password, company.PasswordHash))
            {
                return Unauthorized("Email veya şifre hatalı.");
            }

            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }


        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
