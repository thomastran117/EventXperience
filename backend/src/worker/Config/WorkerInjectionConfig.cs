using backend.Services;

using worker.Interfaces;
using worker.Models;
using worker.Repositories;
using worker.Services;

namespace backend.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IClubRepository, ClubRepository>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }
}
