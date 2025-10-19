using Nearest.Models;

namespace Nearest.Repositories
{
    public interface IAdminRepository
    {
        Task<Admin?> GetByEmailAsync(string email);
        Task<Admin?> GetByIdAsync(int id);
        Task<Admin> AddAsync(Admin admin);
        Task<bool> ExistsAsync(string email);
        Task<bool> IsDefaultAdminExistsAsync();
    }
}
