using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public class DistrictRepository : IDistrictRepository
    {
        private readonly ApplicationDbContext _context;

        public DistrictRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _context.Districts.ToListAsync();
        }

        public async Task<District?> GetByDistrictIdAsync(int districtId)
        {
            return await _context.Districts
                .FirstOrDefaultAsync(d => d.DistrictId == districtId);
        }

        public async Task<District?> GetByIdAsync(string id)
        {
            return await _context.Districts
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<District> AddAsync(District district)
        {
            _context.Districts.Add(district);
            await _context.SaveChangesAsync();
            return district;
        }

        public async Task<District> UpdateAsync(District district)
        {
            _context.Districts.Update(district);
            await _context.SaveChangesAsync();
            return district;
        }

        public async Task<bool> ExistsAsync(int districtId)
        {
            return await _context.Districts
                .AnyAsync(d => d.DistrictId == districtId);
        }
    }
}
