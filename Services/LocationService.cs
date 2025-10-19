using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;

namespace Nearest.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CompanyDto>> GetNearestCompaniesAsync(double latitude, double longitude, int limit = 10)
        {
            var companies = await _context.Companies
                .Where(c => c.IsActive && c.Latitude.HasValue && c.Longitude.HasValue)
                .ToListAsync();

            var companyDtos = _mapper.Map<List<CompanyDto>>(companies);

            // Her firma için mesafeyi hesapla
            foreach (var company in companyDtos)
            {
                if (company.Latitude.HasValue && company.Longitude.HasValue)
                {
                    company.Distance = CalculateDistance(latitude, longitude, 
                        company.Latitude.Value, company.Longitude.Value);
                }
            }

            // Mesafeye göre sırala ve limit kadar getir
            return companyDtos
                .Where(c => c.Distance.HasValue)
                .OrderBy(c => c.Distance)
                .Take(limit)
                .ToList();
        }

        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .Where(c => c.IsActive)
                .ToListAsync();

            return _mapper.Map<List<CompanyDto>>(companies);
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371; // Dünya yarıçapı (km)

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
