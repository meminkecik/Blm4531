namespace Nearest.DTOs
{
    public class AdminAuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public AdminDto Admin { get; set; } = new AdminDto();
        public string Role { get; set; } = "Admin";
    }
}
