namespace Nearest.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public CompanyDto Company { get; set; } = new CompanyDto();
    }
}
