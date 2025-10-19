using System.Text.Json.Serialization;

namespace Nearest.DTOs.Address
{
    public class ProvincesResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ProvincesCityDataDto> Data { get; set; } = new List<ProvincesCityDataDto>();
    }
}
