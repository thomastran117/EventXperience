using Microsoft.EntityFrameworkCore;
using backend.Resources;
using backend.Config;
using backend.Utilities;

namespace backend.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDatabaseContext>(options =>
        {
            options.UseNpgsql(EnvManager.DbConnectionString);
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
