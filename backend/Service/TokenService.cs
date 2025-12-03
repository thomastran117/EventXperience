using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Config;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using System.Security.Cryptography;
using Newtonsoft.Json;
using backend.Utilities;

namespace backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly string JWT_ACCESS_SECRET;
        private readonly TimeSpan JWT_ACCESS_LIFETIME = TimeSpan.FromMinutes(15);
        private const string ISSUER = "EventXperience";
        private const string AUDIENCE = "EventXperienceConsumers";
        private readonly ICacheService _cacheService;
        private readonly TimeSpan REFRESH_TTL = TimeSpan.FromHours(1);
        private readonly TimeSpan VERIFY_TTL = TimeSpan.FromMinutes(30);

        public TokenService(ICacheService cacheService)
        {
            JWT_ACCESS_SECRET = EnvManager.JwtSecretKeyAccess;
            _cacheService = cacheService;
        }

        public string GenerateAccessToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_ACCESS_SECRET));

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Usertype),
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.Add(JWT_ACCESS_LIFETIME),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = ISSUER,
                    Audience = AUDIENCE
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                return _tokenHandler.WriteToken(token);                
            }
            catch (Exception e)
            {
                if (e is AppException) throw;

                Logger.Error($"[TokenService] GenerateAccessToken failed: {e}");
                throw new InternalServerException();
            }
        }

        public async Task<string> GenerateRefreshToken(int userId)
        {
            try
            {
                string token;

                do
                {
                    token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                    string? existing = await _cacheService.GetValueAsync($"refresh:{token}");

                    if (existing == null)
                        break;
                }

                while (true);

                var result = await _cacheService.SetValueAsync(
                    key: $"refresh:{token}",
                    value: userId.ToString(),
                    expiry: REFRESH_TTL
                );

                if (!result) throw new NotAvaliableException();

                return token;
            }
            catch (Exception e)
            {
                if (e is AppException) throw;

                Logger.Error($"[TokenService] GenerateRefreshToken failed: {e}");
                throw new InternalServerException();
            }
        }

        public async Task<int> ValidateRefreshToken(string refreshToken)
        {
            try
            {
                string? idString = await _cacheService.GetValueAsync($"refresh:{refreshToken}");

                if (string.IsNullOrEmpty(idString))
                    throw new UnauthorizedException("Invalid or expired refresh token.");

                int userId = int.Parse(idString);

                var result = await _cacheService.DeleteKeyAsync($"refresh:{refreshToken}");
                if (!result) throw new NotAvaliableException();

                return userId;          
            }
            catch (Exception e)
            {
                if (e is AppException) throw;

                Logger.Error($"[TokenService] ValidateRefreshToken failed: {e}");
                throw new InternalServerException();
            }
        }

        public async Task<string> GenerateVerificationToken(User user)
        {
            try
            {
                string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                var payload = new
                {
                    email = user.Email,
                    password = user.Password,
                    role = user.Usertype
                };

                string serialized = JsonConvert.SerializeObject(payload);

                var result = await _cacheService.SetValueAsync(
                    key: $"verify:{token}",
                    value: serialized,
                    expiry: VERIFY_TTL
                );
                if (!result) throw new NotAvaliableException();

                return token;               
            }
            catch (Exception e)
            {
                if (e is AppException) throw;

                Logger.Error($"[TokenService] GenerateVerificationToken failed: {e}");
                throw new InternalServerException();
            }
        }

        public async Task<User> VerifyVerificationToken(string token)
        {
            try
            {
                string? json = await _cacheService.GetValueAsync($"verify:{token}");

                if (string.IsNullOrEmpty(json))
                    throw new UnauthorizedException("Invalid or expired verification token.");

                var result = await _cacheService.DeleteKeyAsync($"verify:{token}");
                if (!result) throw new NotAvaliableException();

                var draft = JsonConvert.DeserializeObject<User>(json)
                    ?? throw new UnauthorizedException("Invalid verification token payload.");

                return draft;  
            }
            catch (Exception e)
            {
                if (e is AppException) throw;

                Logger.Error($"[TokenService] VerifyVerificationToken failed: {e}");
                throw new InternalServerException();
            }
        }
    }
}
