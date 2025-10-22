using System.Security.Claims;
using backend.Models;
using backend.Common;

namespace backend.Interfaces
{
    public interface ITokenService
    {
        public Token GenerateTokens(User user);
        public UserToken RotateTokens(string oldRefreshToken);
    }
}