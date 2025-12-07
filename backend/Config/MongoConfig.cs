using backend.Resource;
using backend.Utilities;

using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Config
{
    public static class MongoConfig
    {
        public static IServiceCollection AddAppMongo(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IMongoClient>(_ =>
                new MongoClient(EnvManager.MongoConnection));

            services.AddSingleton<MongoResource>();

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                try
                {
                    var client = scope.ServiceProvider.GetRequiredService<IMongoClient>();

                    var database = client.GetDatabase("admin");
                    var command = new BsonDocument("ping", 1);

                    database.RunCommand<BsonDocument>(command);

                    var mongoUrl = new MongoUrl(EnvManager.MongoConnection);

                    Logger.Info(
                        "MongoDB connection successful"
                    );
                }
                catch (Exception ex)
                {
                    Logger.Error($"MongoDB connection error: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            return services;
        }
    }
}
