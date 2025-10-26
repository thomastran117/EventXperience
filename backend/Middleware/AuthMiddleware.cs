using System.Text;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

using backend.Exceptions;
using backend.Common;
using backend.Config;

namespace backend.Middlewares
{
    public static class AuthMiddleware
    {
        private const string ISSUER = "EventXperience";
        private const string AUDIENCE = "EventXperienceConsumers";
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = ISSUER,
                        ValidAudience = AUDIENCE,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(EnvManager.JwtSecretKeyAccess))
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
    public static class ClaimsPrincipalExtensions
    {
        public static UserPayload GetUserPayload(this ClaimsPrincipal user)
        {
            string? idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? emailClaim = user.FindFirst(ClaimTypes.Name)?.Value;
            string? roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(idClaim) || string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedException("Invalid token payload");
            }

            return new UserPayload(int.Parse(idClaim), emailClaim, roleClaim);
        }
    }
}