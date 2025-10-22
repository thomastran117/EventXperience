using backend.Config;

using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using backend.Exceptions;
using backend.Common;

namespace backend.Middlewares
{
    public static class AuthMiddleware
    {
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
                        ValidIssuer = EnvManager.JwtIssuer,
                        ValidAudience = EnvManager.JwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(EnvManager.JwtSecretKey))
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
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = user.FindFirst(ClaimTypes.Name)?.Value;
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(idClaim) || string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedException("Invalid token payload");
            }

            return new UserPayload(int.Parse(idClaim), emailClaim, roleClaim);
        }
    }
}