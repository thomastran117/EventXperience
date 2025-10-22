using backend.Config;

using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend.Middlewares;

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
