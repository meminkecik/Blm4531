using AutoMapper;
using Nearest.DTOs;
using Nearest.Repositories;

namespace Nearest.Services
{
    /// <summary>
    /// Firma (Company) yönetim servisi implementasyonu
    /// SOLID: Single Responsibility - Sadece firma işlemleri
    /// Repository Pattern: Data access için ICompanyRepository kullanır
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Tüm firmaları listeler (Admin için)
        /// </summary>
        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return _mapper.Map<List<CompanyDto>>(companies);
        }

        /// <summary>
        /// ID ile firma getirir
        /// </summary>
        public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            return company != null ? _mapper.Map<CompanyDto>(company) : null;
        }

        /// <summary>
        /// Email ile firma getirir
        /// </summary>
        public async Task<CompanyDto?> GetCompanyByEmailAsync(string email)
        {
            var company = await _companyRepository.GetByEmailAsync(email);
            return company != null ? _mapper.Map<CompanyDto>(company) : null;
        }

        /// <summary>
        /// Firma günceller (Admin için)
        /// </summary>
        public async Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(int id, AdminCompanyUpdateDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(id);
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

            await _companyRepository.UpdateAsync(company);

            return ServiceResult<CompanyDto>.Ok(_mapper.Map<CompanyDto>(company));
        }

        /// <summary>
        /// Firma siler (Admin için)
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteCompanyAsync(int id)
        {
            var deleted = await _companyRepository.DeleteAsync(id);
            
            if (!deleted)
            {
                return ServiceResult<bool>.NotFound("Şirket bulunamadı.");
            }

            return ServiceResult<bool>.Ok(true);
        }

        /// <summary>
        /// Email'in mevcut olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _companyRepository.EmailExistsAsync(email);
        }

        /// <summary>
        /// Telefon numarasının mevcut olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsPhoneExistsAsync(string phoneNumber)
        {
            return await _companyRepository.PhoneExistsAsync(phoneNumber);
        }
    }
}
