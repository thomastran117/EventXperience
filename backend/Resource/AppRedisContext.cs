using StackExchange.Redis;

namespace backend.Resources
{
    public class RedisResource
    {
        public IDatabase Database { get; }

        public RedisResource(IConnectionMultiplexer multiplexer)
        {
            Database = multiplexer.GetDatabase();
        }
    }
}
