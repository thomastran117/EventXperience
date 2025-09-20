using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using backend.Resources;

namespace backend.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddAppRedis(this IServiceCollection services, IConfiguration config)
    {
        var redisConn =
            Environment.GetEnvironmentVariable("REDIS_CONNECTION")
            ?? config["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConn));

        services.AddScoped<RedisResource>();

        return services;
    }
}
