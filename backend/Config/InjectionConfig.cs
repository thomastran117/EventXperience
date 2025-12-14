using backend.Interfaces;
using backend.Queues;
using backend.Repositories;
using backend.Resources;
using backend.Services;
using backend.Utilities;

namespace backend.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddSingleton<IPublisher, Publisher>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOAuthService, OAuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IClubService, ClubService>();
            services.AddScoped<IFileUploadService, FileUploadService>();

            services.AddSingleton<IEmailService, EmailService>();

            services.AddSingleton<ICacheService>(sp =>
            {
                var redisHealth = sp.GetRequiredService<RedisHealth>();

                if (redisHealth.IsAvailable)
                {
                    Logger.Info("Using Redis-backed CacheService.");
                    var redis = sp.GetRequiredService<RedisResource>();
                    return new CacheService(redis);
                }

                Logger.Warn("Using InMemoryCacheService (Redis unavailable).");
                return new InMemoryCacheService();
            });

            return services;
        }
    }
}
