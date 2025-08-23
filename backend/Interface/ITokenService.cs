using System.Security.Claims;
using backend.Models;

namespace backend.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}