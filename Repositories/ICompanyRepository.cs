using Nearest.Models;

namespace Nearest.Repositories
{
    /// <summary>
    /// Company (Firma) Repository Interface
    /// SOLID: Data access işlemleri için ayrı katman
    /// </summary>
    public interface ICompanyRepository
    {
        /// <summary>
        /// ID ile firma getirir
        /// </summary>
        Task<Company?> GetByIdAsync(int id);

        /// <summary>
        /// ID ile firma getirir (çekiciler dahil)
        /// </summary>
        Task<Company?> GetByIdWithTowTrucksAsync(int id);

        /// <summary>
        /// Email ile firma getirir
        /// </summary>
        Task<Company?> GetByEmailAsync(string email);

        /// <summary>
        /// Aktif firma getirir (login için)
        /// </summary>
        Task<Company?> GetActiveByEmailAsync(string email);

        /// <summary>
        /// Tüm firmaları listeler
        /// </summary>
        Task<List<Company>> GetAllAsync();

        /// <summary>
        /// Yeni firma ekler
        /// </summary>
        Task<Company> AddAsync(Company company);

        /// <summary>
        /// Firma günceller
        /// </summary>
        Task<Company> UpdateAsync(Company company);

        /// <summary>
        /// Firma siler
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Email mevcut mu kontrol eder
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Telefon numarası mevcut mu kontrol eder
        /// </summary>
        Task<bool> PhoneExistsAsync(string phoneNumber);
    }
}
