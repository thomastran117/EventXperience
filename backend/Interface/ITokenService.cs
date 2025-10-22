using backend.Common;
using backend.Models;

namespace backend.Interfaces
{
    public interface ITokenService
    {
        public Token GenerateTokens(User user);
        public UserToken RotateTokens(string oldRefreshToken);
    }
}