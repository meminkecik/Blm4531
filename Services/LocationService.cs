using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nearest.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationService> _logger;

        public LocationService(ApplicationDbContext context, IMapper mapper, HttpClient httpClient, ILogger<LocationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CompanyDto>> GetNearestCompaniesAsync(
            double latitude,
            double longitude,
            int limit = 10,
            int? provinceId = null,
            int? districtId = null)
        {
            var baseQuery = _context.Companies
                .Where(c => c.IsActive);

            List<Company> companies = new List<Company>();

            // Step 1: Try to get companies from the user's district
            if (districtId.HasValue)
            {
                companies = await baseQuery
                    .Where(c => c.DistrictId == districtId.Value)
                    .ToListAsync();

                // Step 2: If no companies found in the district, try to find companies in the nearest district
                if (!companies.Any() && provinceId.HasValue)
                {
                    // Get all districts in the province
                    var districtsInProvince = await _context.CityDistricts
                        .Include(cd => cd.District)
                        .Where(cd => cd.City.ProvinceId == provinceId.Value)
                        .Select(cd => new { 
                            DistrictId = cd.District.DistrictId,
                            DistrictName = cd.District.DistrictName
                        })
                        .ToListAsync();

                    // Get companies in each district and calculate the distance to find the nearest district with companies
                    foreach (var district in districtsInProvince)
                    {
                        if (district.DistrictId == districtId.Value)
                            continue; // Skip the user's district as we already checked it

                        var companiesInDistrict = await baseQuery
                            .Where(c => c.DistrictId == district.DistrictId)
                            .ToListAsync();

                        if (companiesInDistrict.Any())
                        {
                            _logger.LogInformation($"Found companies in nearest district: {district.DistrictName}");
                            companies = companiesInDistrict;
                            break;
                        }
                    }
                }
            }

            // Step 3: If still no companies found, try to get companies from the user's province
            if (!companies.Any() && provinceId.HasValue)
            {
                _logger.LogInformation("No companies found in district or nearest districts, trying province level");
                companies = await baseQuery
                    .Where(c => c.ProvinceId == provinceId.Value)
                    .ToListAsync();
            }

            // Step 4: If still no companies found, return random companies
            if (!companies.Any())
            {
                _logger.LogInformation("No companies found in province, returning random companies");
                companies = await baseQuery.ToListAsync();
            }

            var companyDtos = _mapper.Map<List<CompanyDto>>(companies);

            var hasGeoPoint = !double.IsNaN(latitude) && !double.IsNaN(longitude);
            if (hasGeoPoint)
            {
                foreach (var company in companyDtos)
                {
                    if (company.Latitude.HasValue && company.Longitude.HasValue)
                    {
                        company.Distance = CalculateDistance(
                            latitude,
                            longitude,
                            company.Latitude.Value,
                            company.Longitude.Value);
                    }
                    else
                    {
                        company.Distance = null;
                    }
                }

                companyDtos = companyDtos
                    .OrderBy(c => c.Distance ?? double.MaxValue)
                    .Take(limit)
                    .ToList();
            }
            else
            {
                companyDtos = companyDtos
                    .Take(limit)
                    .ToList();
            }

            return companyDtos;
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

        public async Task<(int? provinceId, int? districtId)> GetProvinceAndDistrictFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                // Use Nominatim OpenStreetMap API for reverse geocoding
                string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
                
                // Add a user agent as required by Nominatim usage policy
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Nearest-App");
                
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get address from coordinates: {response.StatusCode}");
                    return (null, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Nominatim response: {content}");
                
                var geocodeResponse = JsonSerializer.Deserialize<NominatimResponse>(content);
                
                if (geocodeResponse == null || geocodeResponse.Address == null)
                {
                    _logger.LogError($"Failed to parse geocode response. Content was: {content}");
                    return (null, null);
                }
                
                _logger.LogInformation($"Parsed address - Province: {geocodeResponse.Address.Province}, City: {geocodeResponse.Address.City}, State: {geocodeResponse.Address.State}, District: {geocodeResponse.Address.District}, Town: {geocodeResponse.Address.Town}, County: {geocodeResponse.Address.County}, Suburb: {geocodeResponse.Address.Suburb}");

                // Extract province and district from the response
                // OpenStreetMap API has inconsistent field usage:
                // - Province can be in "province", "city", or "state" fields
                // - District can be in "district", "town", "suburb", or "county" fields
                string? provinceName = geocodeResponse.Address.Province ?? 
                                      geocodeResponse.Address.City ?? 
                                      geocodeResponse.Address.State;
                
                string? districtName = geocodeResponse.Address.District ?? 
                                      geocodeResponse.Address.Town ?? 
                                      geocodeResponse.Address.Suburb ??
                                      geocodeResponse.Address.County;

                if (string.IsNullOrEmpty(provinceName))
                {
                    _logger.LogWarning($"Province not found for coordinates: {latitude}, {longitude}");
                    return (null, null);
                }

                // Find the province in the database
                var province = await _context.Cities
                    .FirstOrDefaultAsync(c => c.CityName.ToLower() == provinceName.ToLower());

                if (province == null)
                {
                    _logger.LogWarning($"Province '{provinceName}' not found in database");
                    return (null, null);
                }

                int? districtId = null;
                if (!string.IsNullOrEmpty(districtName))
                {
                    // Find the district in the database
                    var district = await _context.Districts
                        .FirstOrDefaultAsync(d => d.DistrictName.ToLower() == districtName.ToLower());

                    if (district != null)
                    {
                        // Verify that the district belongs to the province
                        var cityDistrict = await _context.CityDistricts
                            .FirstOrDefaultAsync(cd => cd.CityId == province.Id && cd.DistrictId == district.Id);

                        if (cityDistrict != null)
                        {
                            districtId = district.DistrictId;
                        }
                    }
                }

                return (province.ProvinceId, districtId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting province and district from coordinates: {latitude}, {longitude}");
                return (null, null);
            }
        }

        // Helper class for deserializing Nominatim response
        private class NominatimResponse
        {
            [JsonPropertyName("address")]
            public NominatimAddress? Address { get; set; }
        }

        private class NominatimAddress
        {
            [JsonPropertyName("city")]
            public string? City { get; set; }
            
            [JsonPropertyName("town")]
            public string? Town { get; set; }
            
            [JsonPropertyName("province")]
            public string? Province { get; set; }
            
            [JsonPropertyName("state")]
            public string? State { get; set; }
            
            [JsonPropertyName("county")]
            public string? County { get; set; }
            
            [JsonPropertyName("district")]
            public string? District { get; set; }
            
            [JsonPropertyName("suburb")]
            public string? Suburb { get; set; }
            
            [JsonPropertyName("neighbourhood")]
            public string? Neighbourhood { get; set; }
        }
    }
}
