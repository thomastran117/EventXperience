using StackExchange.Redis;
using backend.Interfaces;
using backend.Resources;

namespace backend.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _db;

        public CacheService(RedisResource redisResource)
        {
            _db = redisResource.Database;
        }

        public async Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                return await _db.StringSetAsync(key, value, expiry);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetValueAsync(string key)
        {
            try
            {
                var result = await _db.StringGetAsync(key);
                return result.HasValue ? result.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            try
            {
                return await _db.KeyDeleteAsync(key);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch
            {
                return false;
            }
        }
    }
}
