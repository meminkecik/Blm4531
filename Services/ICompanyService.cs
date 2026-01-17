using Nearest.DTOs;

namespace Nearest.Services
{
    /// <summary>
    /// Firma (Company) yönetim servisi interface'i
    /// SOLID: Single Responsibility - Sadece firma işlemleri
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Tüm firmaları listeler (Admin için)
        /// </summary>
        Task<List<CompanyDto>> GetAllCompaniesAsync();

        /// <summary>
        /// ID ile firma getirir
        /// </summary>
        Task<CompanyDto?> GetCompanyByIdAsync(int id);

        /// <summary>
        /// Email ile firma getirir
        /// </summary>
        Task<CompanyDto?> GetCompanyByEmailAsync(string email);

        /// <summary>
        /// Firma günceller (Admin için)
        /// </summary>
        Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(int id, AdminCompanyUpdateDto dto);

        /// <summary>
        /// Firma siler (Admin için)
        /// </summary>
        Task<ServiceResult<bool>> DeleteCompanyAsync(int id);

        /// <summary>
        /// Email'in mevcut olup olmadığını kontrol eder
        /// </summary>
        Task<bool> IsEmailExistsAsync(string email);

        /// <summary>
        /// Telefon numarasının mevcut olup olmadığını kontrol eder
        /// </summary>
        Task<bool> IsPhoneExistsAsync(string phoneNumber);
    }

    /// <summary>
    /// Genel servis sonuç modeli
    /// </summary>
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static ServiceResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
        public static ServiceResult<T> NotFound(string message = "Kayıt bulunamadı") => new() { Success = false, ErrorMessage = message };
    }
}
