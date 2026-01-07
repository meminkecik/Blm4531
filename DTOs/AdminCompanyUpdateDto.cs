using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs
{
    public class AdminCompanyUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public string? FullAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ServiceCity { get; set; }
        public string? ServiceDistrict { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
    }
}
