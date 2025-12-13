using StackExchange.Redis;

namespace backend.Resources
{
    public sealed class RedisHealth
    {
        public bool IsAvailable { get; internal set; }
        public Exception? Failure { get; internal set; }
    }

    public class RedisResource
    {
        public IDatabase Database { get; }

        public RedisResource(IConnectionMultiplexer multiplexer)
        {
            Database = multiplexer.GetDatabase();
        }
    }
}
