using StackExchange.Redis;

using backend.Resources;
using backend.Utilities;

namespace backend.Config
{
    public static class RedisConfig
    {
        public static IServiceCollection AddAppRedis(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(EnvManager.RedisConnection));

            services.AddScoped<RedisResource>();

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                try
                {
                    var mux = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
                    var db = mux.GetDatabase();

                    var latency = db.Ping();
                    Logger.Info($"Redis connection successful (ping: {latency.TotalMilliseconds} ms).");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Redis connection error: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            return services;
        }
    }
}