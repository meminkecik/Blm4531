using AutoMapper;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Repositories;

namespace Nearest.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AdminService(IAdminRepository adminRepository, IJwtService jwtService, IMapper mapper)
        {
            _adminRepository = adminRepository;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<AdminAuthResponseDto?> LoginAsync(AdminLoginDto loginDto)
        {
            var admin = await _adminRepository.GetByEmailAsync(loginDto.Email);
            if (admin == null || !VerifyPassword(loginDto.Password, admin.PasswordHash))
            {
                return null;
            }

            var adminDto = _mapper.Map<AdminDto>(admin);
            var token = GenerateAdminToken(admin);

            return new AdminAuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Admin = adminDto,
                Role = "Admin"
            };
        }

        public async Task<AdminDto?> GetByIdAsync(int id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            return admin != null ? _mapper.Map<AdminDto>(admin) : null;
        }

        public async Task<bool> CreateDefaultAdminAsync()
        {
            if (await _adminRepository.IsDefaultAdminExistsAsync())
            {
                return false; // Zaten mevcut
            }

            var admin = new Admin
            {
                Email = "nearestmek@gmail.com",
                PasswordHash = HashPassword("145236Aa**"),
                FirstName = "Nearest",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _adminRepository.AddAsync(admin);
            return true;
        }

        private string GenerateAdminToken(Admin admin)
        {
            return _jwtService.GenerateToken(admin);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
