using backend.Common;
using backend.Models;

namespace backend.Interfaces
{
    public interface IAuthService
    {
        Task<User> SignUpAsync(string email, string password, string usertype);
        Task<UserToken> LoginAsync(string email, string password);
        Task<UserToken?> GoogleAsync(string token);
        Task<UserToken?> MicrosoftAsync(string email, string password);
        Task<UserToken> HandleTokensAsync(string refreshToken);
        Task HandleLogoutAsync(string refreshToken);
    }
}