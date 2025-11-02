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
    /// <summary>
    /// Kimlik Doğrulama Controller - Firma kayıt ve giriş işlemlerini yönetir
    /// 
    /// Bu controller, oto kurtarma firmalarının sisteme kaydolması ve giriş yapması için
    /// gerekli endpoint'leri sağlar. Şifreler SHA256 ile hash'lenerek saklanır.
    /// Başarılı işlemlerden sonra JWT token döndürülür (7 gün geçerli).
    /// 
    /// Güvenlik:
    /// - Şifreler hash'lenerek veritabanında saklanır
    /// - Email ve telefon numarası unique olmalıdır
    /// - JWT token ile oturum yönetimi sağlanır
    /// </summary>
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

        /// <summary>
        /// Yeni firma kaydı oluşturur
        /// 
        /// Firma kaydı sırasında:
        /// - Email ve telefon numarasının benzersiz olduğu kontrol edilir
        /// - Şifre SHA256 ile hash'lenir
        /// - Adres bilgileri (Province/District) ID'lerden adres servisi ile çözümlenir
        /// - Başarılı kayıt sonrası JWT token oluşturulur ve döndürülür
        /// 
        /// Kayıt için gerekli bilgiler:
        /// - Firma ve temsilci kişisel bilgileri
        /// - Firma adresi ve konum bilgileri (latitude, longitude)
        /// - Hizmet verilen bölgeler
        /// - İletişim bilgileri (email, telefon)
        /// </summary>
        /// <param name="dto">Firma kayıt bilgileri</param>
        /// <returns>JWT token ve firma bilgileri</returns>
        /// <response code="200">Kayıt başarılı, token döndürüldü</response>
        /// <response code="400">Email veya telefon numarası zaten kullanılıyor</response>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(CompanyRegistrationDto dto)
        {
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
        /// Firma giriş yapar
        /// 
        /// Giriş işlemi sırasında:
        /// - Email ve şifre doğrulanır
        /// - Aktif olan (IsActive=true) firmalar giriş yapabilir
        /// - Şifre kontrolü hash karşılaştırması ile yapılır
        /// - Başarılı giriş sonrası JWT token döndürülür (7 gün geçerli)
        /// 
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

        /// <summary>
        /// Şifreyi SHA256 algoritması ile hash'ler
        /// 
        /// Güvenlik için şifreler düz metin olarak saklanmaz.
        /// Base64 encoding ile hash'lenmiş şifre saklanır.
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Girilen şifreyi hash'lenmiş şifre ile karşılaştırır
        /// 
        /// Giriş sırasında kullanıcının girdiği şifre hash'lenerek
        /// veritabanındaki hash değeri ile karşılaştırılır.
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
