using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public interface IDistrictRepository
    {
        Task<IEnumerable<District>> GetAllAsync();
        Task<District?> GetByDistrictIdAsync(int districtId);
        Task<District?> GetByIdAsync(string id);
        Task<District> AddAsync(District district);
        Task<District> UpdateAsync(District district);
        Task<bool> ExistsAsync(int districtId);
    }
}
