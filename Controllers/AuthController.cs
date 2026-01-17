using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Services;

namespace Nearest.Controllers
{
    /// <summary>
    /// Kimlik Doğrulama Controller
    /// 
    /// Bu controller, firma kayıt ve giriş işlemlerini yönetir.
    /// SOLID: Controller sadece HTTP isteklerini karşılar, iş mantığı Service katmanında.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Yeni firma kaydı oluşturur
        /// 
        /// Kayıt için gerekli bilgiler:
        /// - Firma ve temsilci kişisel bilgileri
        /// - Firma adresi ve konum bilgileri (latitude, longitude)
        /// - Hizmet verilen bölgeler
        /// - İletişim bilgileri (email, telefon)
        /// - KVKK açık rıza onayı (zorunlu)
        /// </summary>
        /// <param name="dto">Kayıt bilgileri</param>
        /// <returns>JWT token ve firma bilgileri</returns>
        /// <response code="200">Kayıt başarılı, token döndürüldü</response>
        /// <response code="400">Validasyon hatası veya email/telefon zaten kayıtlı</response>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] CompanyRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = GetClientIpAddress();
            var result = await _authService.RegisterAsync(dto, ipAddress);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Firma girişi yapar
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
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] CompanyLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// İstemci IP adresini alır (KVKK kaydı için)
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
    }
}
