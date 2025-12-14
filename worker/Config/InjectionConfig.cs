using worker.Interfaces;
using worker.Services;

namespace backend.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IEmailService, EmailService>();

            return services;
        }
    }
}
