using Microsoft.EntityFrameworkCore;
using backend.Resources;
using backend.Utilities;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace backend.Config
{
    public static class DatabaseConfig
    {
        public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = EnvManager.DbConnectionString;

            services.AddDbContext<AppDatabaseContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            return services;
        }
    }
}
