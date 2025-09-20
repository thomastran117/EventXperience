using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using backend.Resources;

namespace backend.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetValue<string>("DB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("DB_CONNECTION_STRING environment variable is not set.");

        services.AddDbContext<AppDatabaseContext>(options =>
        {
            options.UseNpgsql(connectionString)
                   .EnableSensitiveDataLogging(false)
                   .LogTo(Console.WriteLine, LogLevel.Warning);
        });

        return services;
    }
}
