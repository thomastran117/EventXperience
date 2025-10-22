using System.Security.Claims;
using backend.Models;

namespace backend.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        public string GenerateRefreshToken(User user);
        public ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
        public (string accessToken, string refreshToken)? RotateRefreshToken(User user, string oldRefreshToken);
        public string? GetAccessTokenFromRefresh(User user, string refreshToken);
    }
}