namespace Nearest.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public string City { get; set; } = string.Empty;
        public int DistrictId { get; set; }
        public string District { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string ServiceCity { get; set; } = string.Empty;
        public string? ServiceDistrict { get; set; }
        public string Email { get; set; } = string.Empty;
        public double? Distance { get; set; } // Kullanıcıya olan mesafe (km)
    }
}
