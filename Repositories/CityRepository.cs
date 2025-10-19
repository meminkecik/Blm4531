using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly ApplicationDbContext _context;

        public CityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetAllAsync()
        {
            return await _context.Cities.ToListAsync();
        }

        public async Task<City?> GetByProvinceIdAsync(int provinceId)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.ProvinceId == provinceId);
        }

        public async Task<City?> GetByIdAsync(string id)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<City> AddAsync(City city)
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
            return city;
        }

        public async Task<City> UpdateAsync(City city)
        {
            _context.Cities.Update(city);
            await _context.SaveChangesAsync();
            return city;
        }

        public async Task<bool> ExistsAsync(int provinceId)
        {
            return await _context.Cities
                .AnyAsync(c => c.ProvinceId == provinceId);
        }
    }
}
