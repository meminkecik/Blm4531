using Nearest.DTOs;

namespace Nearest.Services
{
    public interface IAdminService
    {
        Task<AdminAuthResponseDto?> LoginAsync(AdminLoginDto loginDto);
        Task<AdminDto?> GetByIdAsync(int id);
        Task<bool> CreateDefaultAdminAsync();
    }
}
