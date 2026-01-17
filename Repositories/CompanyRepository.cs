using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.Models;

namespace Nearest.Repositories
{
    /// <summary>
    /// Company (Firma) Repository Implementation
    /// SOLID: Data access işlemleri için ayrı katman
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ID ile firma getirir
        /// </summary>
        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _context.Companies.FindAsync(id);
        }

        /// <summary>
        /// ID ile firma getirir (çekiciler dahil)
        /// </summary>
        public async Task<Company?> GetByIdWithTowTrucksAsync(int id)
        {
            return await _context.Companies
                .Include(c => c.TowTrucks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Email ile firma getirir
        /// </summary>
        public async Task<Company?> GetByEmailAsync(string email)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        /// <summary>
        /// Aktif firma getirir (login için)
        /// </summary>
        public async Task<Company?> GetActiveByEmailAsync(string email)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);
        }

        /// <summary>
        /// Tüm firmaları listeler
        /// </summary>
        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Yeni firma ekler
        /// </summary>
        public async Task<Company> AddAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        /// <summary>
        /// Firma günceller
        /// </summary>
        public async Task<Company> UpdateAsync(Company company)
        {
            company.UpdatedAt = DateTime.UtcNow;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
            return company;
        }

        /// <summary>
        /// Firma siler
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var company = await GetByIdWithTowTrucksAsync(id);
            if (company == null)
            {
                return false;
            }

            // İlişkili çekicileri de sil
            if (company.TowTrucks?.Any() == true)
            {
                _context.TowTrucks.RemoveRange(company.TowTrucks);
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Email mevcut mu kontrol eder
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Companies.AnyAsync(c => c.Email == email);
        }

        /// <summary>
        /// Telefon numarası mevcut mu kontrol eder
        /// </summary>
        public async Task<bool> PhoneExistsAsync(string phoneNumber)
        {
            return await _context.Companies.AnyAsync(c => c.PhoneNumber == phoneNumber);
        }
    }
}
