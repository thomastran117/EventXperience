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

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();

                try
                {
                    if (db.Database.CanConnect())
                    {
                        Logger.Info("Database connection successful.");
                    }
                    else
                    {
                        Logger.Error("Database connection failed.");
                        Environment.Exit(1);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Database connection error: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            return services;
        }
    }
}
