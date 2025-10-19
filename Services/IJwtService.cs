using Nearest.Models;

namespace Nearest.Services
{
    public interface IJwtService
    {
        string GenerateToken(Company company);
        string GenerateToken(Admin admin);
        bool ValidateToken(string token);
    }
}
