using backend.Interfaces;
using backend.Config;
using backend.Common;
using backend.Exceptions;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly string? _googleClientId;
        private readonly string? _microsoftClientId;
        private readonly string? _appleClientId;

        public OAuthService()
        {
            _googleClientId = EnvManager.GoogleClientId;
            _microsoftClientId = EnvManager.MicrosoftClientId;
        }

        public async Task<OAuthUser> VerifyAppleTokenAsync(string appleToken)
        {
            if (_appleClientId == null)
                throw new NotAvaliableException("Apple OAuth is not avaliable to serve this request");

            throw new CustomNotImplementedException("Not implemented yet");
        }

        public async Task<OAuthUser> VerifyGoogleTokenAsync(string googleToken)
        {
            if (_googleClientId == null)
                throw new NotAvaliableException("Google OAuth is not avaliable to serve this request");

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, validationSettings);

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
                throw new NotAvaliableException("Microsoft OAuth is not available to serve this request");

            try
            {
                var jwksUri = "https://login.microsoftonline.com/common/discovery/v2.0/keys";

                var httpClient = new HttpClient();
                var jwks = await httpClient.GetFromJsonAsync<JsonWebKeySet>(jwksUri);

                if (jwks == null)
                    throw new NotAvaliableException("Failed to fetch Microsoft JWKS keys");

                var validationParameters = new TokenValidationParameters
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
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = jwks.Keys
                };

                var handler = new JwtSecurityTokenHandler();
                handler.MapInboundClaims = false;

                var principal = handler.ValidateToken(microsoftToken, validationParameters, out SecurityToken validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null)
                    throw new UnauthorizedException("Invalid Microsoft ID token format");

                var email = principal.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                         ?? principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                         ?? throw new UnauthorizedException("Bad Microsft Token");

                var name = principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value 
                    ?? throw new UnauthorizedException("Bad Microsft Token");
                var sub = principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                    ?? throw new UnauthorizedException("Bad Microsft Token");

                return new OAuthUser(
                    sub,
                    email,
                    name ?? email,
                    "microsoft"
                );
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new UnauthorizedException("Invalid Microsoft ID token: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new NotAvaliableException("Failed to validate Microsoft token: " + ex.Message);
            }
        }
    }
}
