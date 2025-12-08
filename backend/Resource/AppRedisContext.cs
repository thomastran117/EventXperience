using StackExchange.Redis;

namespace backend.Resources
{
    public class RedisResource
    {
        public IDatabase Database { get; }
        public IConnectionMultiplexer Multiplexer { get; }

        public RedisResource(IConnectionMultiplexer multiplexer)
        {
            Multiplexer = multiplexer;
            Database = multiplexer.GetDatabase();
        }
    }
}
