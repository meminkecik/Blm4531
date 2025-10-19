namespace Nearest.DTOs.Address
{
    public class DistrictResponseDto
    {
        public string Status { get; set; } = "SUCCESS";
        public List<DistrictDto> Data { get; set; } = new List<DistrictDto>();
    }
}
