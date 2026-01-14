using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Services
{
    /// <summary>
    /// Firma (Company) yönetim servisi implementasyonu
    /// SOLID: Single Responsibility - Sadece firma işlemleri
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CompanyService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Tüm firmaları listeler (Admin için)
        /// </summary>
        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<CompanyDto>>(companies);
        }

        /// <summary>
        /// ID ile firma getirir
        /// </summary>
        public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            return company != null ? _mapper.Map<CompanyDto>(company) : null;
        }

        /// <summary>
        /// Email ile firma getirir
        /// </summary>
        public async Task<CompanyDto?> GetCompanyByEmailAsync(string email)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == email);
            return company != null ? _mapper.Map<CompanyDto>(company) : null;
        }

        /// <summary>
        /// Firma günceller (Admin için)
        /// </summary>
        public async Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(int id, AdminCompanyUpdateDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return ServiceResult<CompanyDto>.NotFound("Şirket bulunamadı.");
            }

            // Sadece gönderilen alanları güncelle
            if (!string.IsNullOrEmpty(dto.FirstName))
                company.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                company.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.CompanyName))
                company.CompanyName = dto.CompanyName;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                company.PhoneNumber = dto.PhoneNumber;

            if (dto.ProvinceId.HasValue)
                company.ProvinceId = dto.ProvinceId.Value;

            if (dto.DistrictId.HasValue)
                company.DistrictId = dto.DistrictId.Value;

            if (!string.IsNullOrEmpty(dto.FullAddress))
                company.FullAddress = dto.FullAddress;

            if (dto.Latitude.HasValue)
                company.Latitude = dto.Latitude;

            if (dto.Longitude.HasValue)
                company.Longitude = dto.Longitude;

            if (!string.IsNullOrEmpty(dto.ServiceCity))
                company.ServiceCity = dto.ServiceCity;

            if (!string.IsNullOrEmpty(dto.ServiceDistrict))
                company.ServiceDistrict = dto.ServiceDistrict;

            if (!string.IsNullOrEmpty(dto.Email))
                company.Email = dto.Email;

            if (dto.IsActive.HasValue)
                company.IsActive = dto.IsActive.Value;

            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult<CompanyDto>.Ok(_mapper.Map<CompanyDto>(company));
        }

        /// <summary>
        /// Firma siler (Admin için)
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.TowTrucks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return ServiceResult<bool>.NotFound("Şirket bulunamadı.");
            }

            // İlişkili çekicileri de sil
            if (company.TowTrucks?.Any() == true)
            {
                _context.TowTrucks.RemoveRange(company.TowTrucks);
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        /// <summary>
        /// Email'in mevcut olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Companies.AnyAsync(c => c.Email == email);
        }

        /// <summary>
        /// Telefon numarasının mevcut olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsPhoneExistsAsync(string phoneNumber)
        {
            return await _context.Companies.AnyAsync(c => c.PhoneNumber == phoneNumber);
        }
    }
}
