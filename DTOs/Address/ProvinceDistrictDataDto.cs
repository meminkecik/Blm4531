using System.Text.Json.Serialization;

namespace Nearest.DTOs.Address
{
    public class ProvinceDistrictDataDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
