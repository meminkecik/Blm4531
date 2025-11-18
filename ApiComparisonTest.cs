using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nearest
{
    // This class is used to test and compare the naming patterns between OpenStreetMap API and turkiyeapi.dev
    public class ApiComparisonTest
    {
        private readonly HttpClient _httpClient;

        public ApiComparisonTest()
        {
            _httpClient = new HttpClient();
            // Add a user agent as required by Nominatim usage policy
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Nearest-App-Test");
        }

        public async Task RunComparisonTest()
        {
            // Test coordinates for different cities in Turkey
            var testCoordinates = new List<(string name, double lat, double lon)>
            {
                ("Istanbul", 41.0082, 29.0094),
                ("Ankara", 39.9208, 32.8541),
                ("Izmir", 38.4237, 27.1428),
                ("Antalya", 36.8969, 30.7133),
                ("Bursa", 40.1885, 29.0610)
            };

            Console.WriteLine("=== API Comparison Test ===");
            Console.WriteLine("Comparing OpenStreetMap API with turkiyeapi.dev API\n");

            foreach (var (name, lat, lon) in testCoordinates)
            {
                Console.WriteLine($"Testing coordinates for {name}: ({lat}, {lon})");
                
                // Get data from OpenStreetMap API
                var osmData = await GetOpenStreetMapData(lat, lon);
                
                // Get data from turkiyeapi.dev API
                var turkiyeApiData = await GetTurkiyeApiData();
                
                // Find matching province in turkiyeapi.dev data
                var matchingProvince = FindMatchingProvince(osmData.provinceName, turkiyeApiData);
                
                Console.WriteLine($"OpenStreetMap Province: {osmData.provinceName}");
                Console.WriteLine($"OpenStreetMap District: {osmData.districtName}");
                
                if (matchingProvince != null)
                {
                    Console.WriteLine($"Matching turkiyeapi.dev Province: {matchingProvince.Name}");
                    
                    // Find matching district
                    var matchingDistrict = FindMatchingDistrict(osmData.districtName, matchingProvince);
                    if (matchingDistrict != null)
                    {
                        Console.WriteLine($"Matching turkiyeapi.dev District: {matchingDistrict.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"No matching district found in turkiyeapi.dev API");
                    }
                }
                else
                {
                    Console.WriteLine($"No matching province found in turkiyeapi.dev API");
                }
                
                Console.WriteLine("----------------------------");
            }
        }

        private async Task<(string provinceName, string districtName)> GetOpenStreetMapData(double lat, double lon)
        {
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lon}&zoom=18&addressdetails=1";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<NominatimResponse>(content);
            
            if (geocodeResponse?.Address == null)
            {
                return (string.Empty, string.Empty);
            }
            
            string provinceName = geocodeResponse.Address.Province ?? 
                                 geocodeResponse.Address.State ?? 
                                 string.Empty;
            
            string districtName = geocodeResponse.Address.District ?? 
                                 geocodeResponse.Address.County ?? 
                                 string.Empty;
            
            return (provinceName, districtName);
        }

        private async Task<List<ProvinceData>> GetTurkiyeApiData()
        {
            const string url = "https://turkiyeapi.dev/api/v1/provinces";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var provincesResponse = JsonSerializer.Deserialize<ProvincesResponse>(content);
            
            return provincesResponse?.Data ?? new List<ProvinceData>();
        }

        private ProvinceData FindMatchingProvince(string osmProvinceName, List<ProvinceData> provinces)
        {
            if (string.IsNullOrEmpty(osmProvinceName))
                return null;
                
            return provinces.FirstOrDefault(p => 
                string.Equals(p.Name, osmProvinceName, StringComparison.OrdinalIgnoreCase));
        }

        private DistrictData FindMatchingDistrict(string osmDistrictName, ProvinceData province)
        {
            if (string.IsNullOrEmpty(osmDistrictName) || province?.Districts == null)
                return null;
                
            return province.Districts.FirstOrDefault(d => 
                string.Equals(d.Name, osmDistrictName, StringComparison.OrdinalIgnoreCase));
        }

        // Helper classes for deserializing API responses
        private class NominatimResponse
        {
            public NominatimAddress Address { get; set; }
        }

        private class NominatimAddress
        {
            public string City { get; set; }
            public string Province { get; set; }
            public string State { get; set; }
            public string County { get; set; }
            public string District { get; set; }
        }

        private class ProvincesResponse
        {
            public List<ProvinceData> Data { get; set; }
        }

        private class ProvinceData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<DistrictData> Districts { get; set; }
        }

        private class DistrictData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    // Main program to run the test
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var test = new ApiComparisonTest();
            await test.RunComparisonTest();
        }
    }
}