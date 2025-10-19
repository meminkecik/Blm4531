using System.Text.Json.Serialization;

namespace Nearest.DTOs.Address
{
    public class ProvincesCityDataDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("districts")]
        public List<ProvinceDistrictDataDto> Districts { get; set; } = new List<ProvinceDistrictDataDto>();
    }
}
