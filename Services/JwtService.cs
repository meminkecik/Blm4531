using Microsoft.IdentityModel.Tokens;
using Nearest.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nearest.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private byte[] GetKey()
        {
            var secretKey = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Jwt:Key ayarı appsettings.json dosyasında bulunamadı!");
            }

            return Encoding.ASCII.GetBytes(secretKey);
        }

        public string GenerateToken(Company company)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetKey();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, company.Id.ToString()),
                    new Claim(ClaimTypes.Email, company.Email),
                    new Claim(ClaimTypes.Name, company.CompanyName),
                    new Claim("CompanyId", company.Id.ToString()),
                    new Claim("Role", "Company")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateToken(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetKey();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                    new Claim("AdminId", admin.Id.ToString()),
                    new Claim("Role", "Admin")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = GetKey();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Eğer Issuer (yayıncı) kontrolü yapacaksan true yapmalısın
                    ValidateAudience = false, // Eğer Audience (hedef kitle) kontrolü yapacaksan true yapmalısın
                    ClockSkew = TimeSpan.Zero // Token süresi dolduğu an geçersiz olsun (normalde 5dk opsiyonel süre tanır)
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}