namespace Nearest.DTOs.Address
{
    public class CityResponseDto
    {
        public string Status { get; set; } = "SUCCESS";
        public List<CityDto> Data { get; set; } = new List<CityDto>();
    }
}
