using Nearest.DTOs;

namespace Nearest.Services
{
    /// <summary>
    /// Kimlik doğrulama servisi interface'i
    /// SOLID: Single Responsibility - Sadece authentication işlemleri
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Yeni firma kaydı oluşturur
        /// </summary>
        /// <param name="dto">Kayıt bilgileri</param>
        /// <param name="ipAddress">İstemci IP adresi (KVKK için)</param>
        /// <returns>Token ve firma bilgisi veya hata mesajı</returns>
        Task<AuthServiceResult> RegisterAsync(CompanyRegistrationDto dto, string ipAddress);

        /// <summary>
        /// Firma girişi yapar
        /// </summary>
        /// <param name="dto">Giriş bilgileri</param>
        /// <returns>Token ve firma bilgisi veya hata mesajı</returns>
        Task<AuthServiceResult> LoginAsync(CompanyLoginDto dto);

        /// <summary>
        /// Şifre hash'ler (SHA256)
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Şifre doğrular
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }

    /// <summary>
    /// Auth servisi sonuç modeli
    /// </summary>
    public class AuthServiceResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public AuthResponseDto? Data { get; set; }

        public static AuthServiceResult Ok(AuthResponseDto data) => new() { Success = true, Data = data };
        public static AuthServiceResult Fail(string message) => new() { Success = false, ErrorMessage = message };
    }
}
