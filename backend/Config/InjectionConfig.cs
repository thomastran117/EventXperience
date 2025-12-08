using backend.Interfaces;
using backend.Repositories;
using backend.Services;

namespace backend.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOAuthService, OAuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IClubService, ClubService>();
            services.AddScoped<IFileUploadService, FileUploadService>();

            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }
}
