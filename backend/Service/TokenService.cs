using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Common;
using backend.Interfaces;
using Microsoft.IdentityModel.Tokens;
using backend.Config;
using backend.Exceptions;

namespace backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public TokenService() { }
        public UserToken RotateTokens(string oldRefreshToken)
        {
            var principal = ValidateRefreshToken(oldRefreshToken)
                ?? throw new UnauthorizedException("Invalid or expired refresh token");

            var user = CreateUserFromClaims(principal);

            var newAccess = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken(user);

            var token = new Token(newAccess, newRefresh);
            
            return new UserToken(token, user);
        }
        public Token GenerateTokens(User user)
        {
            var newAccess = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken(user);

            var token = new Token(newAccess, newRefresh);

            return token;
        }
        private static User CreateUserFromClaims(ClaimsPrincipal principal)
        {
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Name);
            var role = principal.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                throw new UnauthorizedException("Invalid refresh token claims");

            return new User
            {
                Id = int.Parse(id),
                Email = email,
                Usertype = role
            };
        }
        
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Usertype),
                    new Claim("token_type", "access")
                ]),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = EnvManager.JwtIssuer,
                Audience = EnvManager.JwtAudience
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(EnvManager.JwtSecretKeyRefresh);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Usertype),
                    new Claim("token_type", "refresh")
                ]),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = EnvManager.JwtIssuer,
                Audience = EnvManager.JwtAudience
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
        private ClaimsPrincipal? ValidateAccessToken(string token)
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

        private ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
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
    }
}