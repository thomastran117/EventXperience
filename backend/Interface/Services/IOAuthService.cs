using backend.Common;

namespace backend.Interfaces
{
    public interface IOAuthService
    {
        Task<OAuthUser> VerifyGoogleTokenAsync(string googleToken);
        Task<OAuthUser> VerifyMicrosoftTokenAsync(string microsoftToken);
        Task<OAuthUser> VerifyAppleTokenAsync(string appleToken);
    }
}
