using System.Text;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using backend.Common;
using backend.Config;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;

namespace backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly string JWT_ACCESS_SECRET;
        private readonly string JWT_REFRESH_SECRET;
        private readonly string JWT_VERIFY_SECRET;
        private readonly TimeSpan JWT_ACCESS_LIFETIME = TimeSpan.FromMinutes(15);
        private readonly TimeSpan JWT_REFESH_LIFETIME = TimeSpan.FromMinutes(60);
        private readonly TimeSpan JWT_VERIFY_LIFETIME = TimeSpan.FromMinutes(30);
        private const string ISSUER = "EventXperience";
        private const string AUDIENCE = "EventXperienceConsumers";
        private readonly ICacheService _cacheService;

        public TokenService(ICacheService cacheService)
        {
            JWT_ACCESS_SECRET = EnvManager.JwtSecretKeyAccess;
            JWT_REFRESH_SECRET = EnvManager.JwtSecretKeyRefresh;
            JWT_VERIFY_SECRET = EnvManager.JwtSecretKeyVerify;
            _cacheService = cacheService;
        }
        public async Task<UserToken> RotateTokens(string oldRefreshToken)
        {
            var user = await ValidateRefreshToken(oldRefreshToken);

            var newAccess = GenerateAccessJwtToken(user);
            var newRefresh = await GenerateRefreshToken(user);

            var token = new Token(newAccess, newRefresh);

            return new UserToken(token, user);
        }
        public async Task<Token> GenerateTokens(User user)
        {
            var newAccess = GenerateAccessJwtToken(user);
            var newRefresh = await GenerateRefreshToken(user);

            var token = new Token(newAccess, newRefresh);

            return token;
        }

        private string GenerateAccessJwtToken(User user)
        {
            return GenerateJwt(user, "access");
        }

        private async Task<string> GenerateRefreshToken(User user)
        {
            var token = GenerateJwt(user, "refresh");
            await _cacheService.SetValueAsync($"refresh:{user.Id}", token, JWT_VERIFY_LIFETIME);
            return token;
        }
        private ClaimsPrincipal? ValidateAccessToken(string token)
        {
            return ValidateJwt(token, "access");
        }

        private async Task<User> ValidateRefreshToken(string refreshToken)
        {
            var principal = ValidateJwt(refreshToken, "refresh")
                ?? throw new UnauthorizedException("Invalid or expired verify token");

            var user = CreateUserFromClaims(principal);

            var cachedToken = await _cacheService.GetValueAsync($"refresh:{user.Id}");
            if (string.IsNullOrEmpty(cachedToken) || cachedToken != refreshToken)
                throw new UnauthorizedException("Refresh token invalid or already used.");

            await _cacheService.DeleteKeyAsync($"refresh:{user.Id}");

            return user;
        }

        public async Task<bool> LogoutToken(string refreshToken)
        {
            var principal = ValidateJwt(refreshToken, "refresh")
                ?? throw new UnauthorizedException("Invalid or expired verify token");

            var user = CreateUserFromClaims(principal);

            var cachedToken = await _cacheService.GetValueAsync($"refresh:{user.Id}");
            if (string.IsNullOrEmpty(cachedToken) || cachedToken != refreshToken)
                throw new UnauthorizedException("Refresh token invalid or already used.");

            await _cacheService.DeleteKeyAsync($"refresh:{user.Id}");

            return true;
        }

        public async Task<string> GenerateVerificationToken(User user)
        {
            var token = GenerateJwt(user, "verify");
            await _cacheService.SetValueAsync($"verify:{user.Id}", token, JWT_VERIFY_LIFETIME);
            return token;
        }

        public async Task<User> VerifyVerificationToken(string verifyToken)
        {
            var principal = ValidateJwt(verifyToken, "verify")
                ?? throw new UnauthorizedException("Invalid or expired verify token");

            var user = CreateUserFromClaims(principal);

            var cachedToken = await _cacheService.GetValueAsync($"verify:{user.Id}");
            if (string.IsNullOrEmpty(cachedToken) || cachedToken != verifyToken)
                throw new UnauthorizedException("Verification token invalid or already used.");

            await _cacheService.DeleteKeyAsync($"verify:{user.Id}");

            return user;
        }

        public string GenerateJwt(User user, string type)
        {
            type = type.ToLowerInvariant();

            string secret = type switch
            {
                "access" => JWT_ACCESS_SECRET,
                "refresh" => JWT_REFRESH_SECRET,
                "verify" => JWT_VERIFY_SECRET,
                _ => throw new ArgumentException($"Invalid token type: {type}")
            };

            TimeSpan expiry = type switch
            {
                "access" => JWT_ACCESS_LIFETIME,
                "refresh" => JWT_REFESH_LIFETIME,
                "verify" => JWT_VERIFY_LIFETIME,
                _ => TimeSpan.Zero
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Usertype),
                new("token_type", type)
            };

            if (type == "verify" && !string.IsNullOrEmpty(user.Password))
                claims.Add(new Claim("password", user.Password));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(expiry),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = ISSUER,
                Audience = AUDIENCE
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal? ValidateJwt(string token, string type)
        {
            type = type.ToLowerInvariant();

            string secret = type switch
            {
                "access" => JWT_ACCESS_SECRET,
                "refresh" => JWT_REFRESH_SECRET,
                "verify" => JWT_VERIFY_SECRET,
                _ => throw new ArgumentException($"Invalid token type: {type}")
            };

            var key = Encoding.UTF8.GetBytes(secret);

            try
            {
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = ISSUER,
                    ValidAudience = AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                if (principal.HasClaim(c => c.Type == "token_type" && c.Value == type))
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
        private User CreateUserFromClaims(ClaimsPrincipal principal)
        {
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Name);
            var role = principal.FindFirstValue(ClaimTypes.Role);
            var password = principal.FindFirst("password")?.Value;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                throw new UnauthorizedException("Invalid refresh token claims");

            return new User
            {
                Id = int.Parse(id),
                Email = email,
                Usertype = role,
                Password = password
            };
        }
    }
}