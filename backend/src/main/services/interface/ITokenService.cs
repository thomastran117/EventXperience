using backend.main.models.core;

namespace backend.main.services.interfaces
{
    public interface ITokenService
    {
        public string GenerateAccessToken(User user);
        public Task<string> GenerateRefreshToken(int userId);
        public Task<string> GenerateVerificationToken(User user);
        public Task<User> VerifyVerificationToken(string verifyToken);
        public Task<int> ValidateRefreshToken(string refreshToken);
        public Task<string?> VerificationTokenExist(string email);
    }
}
