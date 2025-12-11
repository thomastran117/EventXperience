using System.IdentityModel.Tokens.Jwt;

using backend.Common;
using backend.Config;
using backend.Exceptions;
using backend.Interfaces;

using Google.Apis.Auth;

using Microsoft.IdentityModel.Tokens;

namespace backend.Services
{
    public class OAuthService : BaseService, IOAuthService
    {
        private readonly string? _googleClientId;
        private readonly string? _microsoftClientId;

        public OAuthService(HttpClient? httpClient = null)
            : base(httpClient)
        {
            _googleClientId = EnvManager.GoogleClientId;
            _microsoftClientId = EnvManager.MicrosoftClientId;
        }

        public Task<OAuthUser> VerifyAppleTokenAsync(string appleToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OAuthUser> VerifyGoogleTokenAsync(string googleToken)
        {
            if (_googleClientId == null)
                throw new NotAvaliableException("Google OAuth is not available");

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };

            var payload = await ExecuteResilientHttpAsync(async () =>
                await GoogleJsonWebSignature.ValidateAsync(googleToken, settings)
            );

            return new OAuthUser(
                payload.Subject,
                payload.Email,
                payload.Name ?? payload.Email,
                "google"
            );
        }

        public async Task<OAuthUser> VerifyMicrosoftTokenAsync(string microsoftToken)
        {
            if (_microsoftClientId == null)
                throw new NotAvaliableException("Microsoft OAuth is not available");

            var jwksUri = "https://login.microsoftonline.com/common/discovery/v2.0/keys";

            // Fetch JWKS through BaseService resilient wrapper
            var jwks = await ExecuteResilientHttpAsync(async () =>
            {
                return await Http.GetFromJsonAsync<JsonWebKeySet>(jwksUri)
                    ?? throw new Exception("Invalid JWKS response");
            });

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    "https://login.microsoftonline.com/{tenantid}/v2.0",
                    "https://login.microsoftonline.com/common/v2.0",
                    "https://login.microsoftonline.com/organizations/v2.0",
                    "https://login.microsoftonline.com/consumers/v2.0",
                },
                ValidateAudience = true,
                ValidAudience = _microsoftClientId,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                IssuerSigningKeys = jwks.Keys
            };

            var handler = new JwtSecurityTokenHandler();
            handler.MapInboundClaims = false;

            var principal = handler.ValidateToken(microsoftToken, validationParams, out var validated);

            var email =
                principal.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ??
                principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value ??
                throw new UnauthorizedException("Missing Microsoft email claim");

            var name =
                principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                email;

            var sub =
                principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ??
                throw new UnauthorizedException("Missing Microsoft sub claim");

            return new OAuthUser(sub, email, name, "microsoft");
        }
    }
}
