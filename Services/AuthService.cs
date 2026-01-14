using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;
using System.Security.Cryptography;
using System.Text;

namespace Nearest.Services
{
    /// <summary>
    /// Kimlik doğrulama servisi implementasyonu
    /// SOLID: Single Responsibility - Sadece authentication işlemleri
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IAddressService _addressService;

        public AuthService(
            ApplicationDbContext context,
            IMapper mapper,
            IJwtService jwtService,
            IAddressService addressService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _addressService = addressService;
        }

        /// <summary>
        /// Yeni firma kaydı oluşturur
        /// </summary>
        public async Task<AuthServiceResult> RegisterAsync(CompanyRegistrationDto dto, string ipAddress)
        {
            // 1. KVKK onayı kontrolü
            if (!dto.KvkkConsent)
            {
                return AuthServiceResult.Fail("KVKK açık rıza onayı zorunludur.");
            }

            // 2. Email benzersizlik kontrolü
            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return AuthServiceResult.Fail("Bu email adresi zaten kullanılıyor.");
            }

            // 3. Telefon benzersizlik kontrolü
            if (await _context.Companies.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            {
                return AuthServiceResult.Fail("Bu telefon numarası zaten kullanılıyor.");
            }

            // 4. DTO'dan Entity'ye dönüşüm
            var company = _mapper.Map<Company>(dto);

            // 5. Province/District isimlerini adres sisteminden çöz
            if (dto.ProvinceId > 0)
            {
                var cityName = await _addressService.GetCityNameAsync(dto.ProvinceId);
                if (!string.IsNullOrEmpty(cityName))
                {
                    company.City = cityName;
                    company.ProvinceId = dto.ProvinceId;
                }
            }

            if (dto.DistrictId > 0)
            {
                var districtName = await _addressService.GetDistrictNameAsync(dto.DistrictId);
                if (!string.IsNullOrEmpty(districtName))
                {
                    company.District = districtName;
                    company.DistrictId = dto.DistrictId;
                }
            }

            // 6. Şifre hashleme
            company.PasswordHash = HashPassword(dto.Password);

            // 7. KVKK bilgilerini kaydet
            company.KvkkConsent = true;
            company.KvkkConsentDate = DateTime.UtcNow;
            company.KvkkConsentVersion = dto.KvkkConsentVersion ?? "1.0";
            company.KvkkConsentIpAddress = ipAddress;

            // 8. Zaman damgaları
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;
            company.IsActive = true;

            // 9. Veritabanına kaydet
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // 10. Response oluştur
            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return AuthServiceResult.Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }

        /// <summary>
        /// Firma girişi yapar
        /// </summary>
        public async Task<AuthServiceResult> LoginAsync(CompanyLoginDto dto)
        {
            // 1. Email ile firmayı bul
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.Email && c.IsActive);

            // 2. Firma bulunamadı veya şifre yanlış
            if (company == null || !VerifyPassword(dto.Password, company.PasswordHash))
            {
                return AuthServiceResult.Fail("Email veya şifre hatalı.");
            }

            // 3. Token oluştur ve döndür
            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return AuthServiceResult.Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }

        /// <summary>
        /// Şifre hash'ler (SHA256)
        /// </summary>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Şifre doğrular
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
