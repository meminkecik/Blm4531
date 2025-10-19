using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public interface ICityRepository
    {
        Task<IEnumerable<City>> GetAllAsync();
        Task<City?> GetByProvinceIdAsync(int provinceId);
        Task<City?> GetByIdAsync(string id);
        Task<City> AddAsync(City city);
        Task<City> UpdateAsync(City city);
        Task<bool> ExistsAsync(int provinceId);
    }
}
