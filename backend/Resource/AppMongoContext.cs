using backend.Config;

using MongoDB.Driver;

namespace backend.Resource
{
    public class MongoResource
    {
        public IMongoDatabase Database { get; }

        public MongoResource(IMongoClient client)
        {
            var mongoUrl = new MongoUrl(EnvManager.MongoConnection);
            Database = client.GetDatabase(mongoUrl.DatabaseName);
        }
    }
}
