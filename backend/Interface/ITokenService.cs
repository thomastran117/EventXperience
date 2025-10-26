using backend.Common;
using backend.Models;

namespace backend.Interfaces
{
    public interface ITokenService
    {
        public Task<Token> GenerateTokens(User user);
        public Task<UserToken> RotateTokens(string refreshToken);
        public Task<bool> LogoutToken(string refreshToken);
        public Task<string> GenerateVerificationToken(User user);
        public Task<User> VerifyVerificationToken(string verifyToken);
    }
}