using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public class CityDistrictRepository : ICityDistrictRepository
    {
        private readonly ApplicationDbContext _context;

        public CityDistrictRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CityDistrict>> GetAllAsync()
        {
            return await _context.CityDistricts
                .Include(cd => cd.City)
                .Include(cd => cd.District)
                .ToListAsync();
        }

        public async Task<CityDistrict?> GetByCityAndDistrictAsync(City city, District district)
        {
            return await _context.CityDistricts
                .Include(cd => cd.City)
                .Include(cd => cd.District)
                .FirstOrDefaultAsync(cd => cd.CityId == city.Id && cd.DistrictId == district.Id);
        }

        public async Task<IEnumerable<CityDistrict>> GetByCityAsync(City city)
        {
            return await _context.CityDistricts
                .Include(cd => cd.City)
                .Include(cd => cd.District)
                .Where(cd => cd.CityId == city.Id)
                .ToListAsync();
        }

        public async Task<CityDistrict> AddAsync(CityDistrict cityDistrict)
        {
            _context.CityDistricts.Add(cityDistrict);
            await _context.SaveChangesAsync();
            return cityDistrict;
        }

        public async Task<CityDistrict> UpdateAsync(CityDistrict cityDistrict)
        {
            _context.CityDistricts.Update(cityDistrict);
            await _context.SaveChangesAsync();
            return cityDistrict;
        }

        public async Task<bool> ExistsAsync(City city, District district)
        {
            return await _context.CityDistricts
                .AnyAsync(cd => cd.CityId == city.Id && cd.DistrictId == district.Id);
        }
    }
}
