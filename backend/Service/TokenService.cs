using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Interfaces;
using Microsoft.IdentityModel.Tokens;
using backend.Config;

namespace backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public TokenService() { }

        public string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Usertype)
                ]),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = EnvManager.JwtIssuer,
                Audience = EnvManager.JwtAudience
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKey);

            try
            {
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = EnvManager.JwtIssuer,
                    ValidAudience = EnvManager.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string GenerateRefreshToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKeyRefresh);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("token_type", "refresh")
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = EnvManager.JwtIssuer,
                Audience = EnvManager.JwtAudience
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKeyRefresh);

            try
            {
                var principal = _tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = EnvManager.JwtIssuer,
                    ValidAudience = EnvManager.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                if (principal.HasClaim(c => c.Type == "token_type" && c.Value == "refresh"))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public (string accessToken, string refreshToken)? RotateRefreshToken(User user, string oldRefreshToken)
        {
            var principal = ValidateRefreshToken(oldRefreshToken);
            if (principal == null) return null;

            var newAccess = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken(user);

            return (newAccess, newRefresh);
        }

        public string? GetAccessTokenFromRefresh(User user, string refreshToken)
        {
            var principal = ValidateRefreshToken(refreshToken);
            if (principal == null) return null;

            return GenerateJwtToken(user);
        }
    }
}