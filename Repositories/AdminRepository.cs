using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.Models;

namespace Nearest.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetByEmailAsync(string email)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
        }

        public async Task<Admin?> GetByIdAsync(int id)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<Admin> AddAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Admins
                .AnyAsync(a => a.Email == email);
        }

        public async Task<bool> IsDefaultAdminExistsAsync()
        {
            return await _context.Admins
                .AnyAsync(a => a.Email == "nearestmek@gmail.com");
        }
    }
}
