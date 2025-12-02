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

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            try { return await _db.StringIncrementAsync(key, value); }
            catch { return 0; }
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            try { return await _db.StringDecrementAsync(key, value); }
            catch { return 0; }
        }

        public async Task<bool> HashSetAsync(string key, string field, string value)
        {
            try { return await _db.HashSetAsync(key, field, value); }
            catch { return false; }
        }

        public async Task<string?> HashGetAsync(string key, string field)
        {
            try
            {
                var value = await _db.HashGetAsync(key, field);
                return value.HasValue ? value.ToString() : null;
            }
            catch { return null; }
        }

        public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            try
            {
                var entries = await _db.HashGetAllAsync(key);
                return entries.ToDictionary(
                    x => x.Name.ToString(),
                    x => x.Value.ToString()
                );
            }
            catch { return new Dictionary<string, string>(); }
        }

        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            try { return await _db.HashDeleteAsync(key, field); }
            catch { return false; }
        }

        public async Task<bool> SetAddAsync(string key, string value)
        {
            try { return await _db.SetAddAsync(key, value); }
            catch { return false; }
        }

        public async Task<bool> SetRemoveAsync(string key, string value)
        {
            try { return await _db.SetRemoveAsync(key, value); }
            catch { return false; }
        }

        public async Task<string[]> SetMembersAsync(string key)
        {
            try
            {
                var members = await _db.SetMembersAsync(key);
                return members.Select(s => s.ToString()).ToArray();
            }
            catch { return Array.Empty<string>(); }
        }

        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            try { return await _db.ListLeftPushAsync(key, value); }
            catch { return 0; }
        }

        public async Task<long> ListRightPushAsync(string key, string value)
        {
            try { return await _db.ListRightPushAsync(key, value); }
            catch { return 0; }
        }

        public async Task<string?> ListLeftPopAsync(string key)
        {
            try
            {
                var v = await _db.ListLeftPopAsync(key);
                return v.HasValue ? v.ToString() : null;
            }
            catch { return null; }
        }

        public async Task<string?> ListRightPopAsync(string key)
        {
            try
            {
                var v = await _db.ListRightPopAsync(key);
                return v.HasValue ? v.ToString() : null;
            }
            catch { return null; }
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            try { return await _db.KeyDeleteAsync(key); }
            catch { return false; }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try { return await _db.KeyExistsAsync(key); }
            catch { return false; }
        }

        public async Task<TimeSpan?> GetTTLAsync(string key)
        {
            try { return await _db.KeyTimeToLiveAsync(key); }
            catch { return null; }
        }

        public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        {
            try { return await _db.KeyExpireAsync(key, expiry); }
            catch { return false; }
        }

        public IEnumerable<string> ScanKeys(IServer server, string pattern)
        {
            foreach (var key in server.Keys(pattern: pattern))
            {
                yield return key.ToString();
            }
        }

        public async Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry)
        {
            try
            {
                return await _db.StringSetAsync(
                    key, 
                    value, 
                    expiry,
                    when: When.NotExists
                );
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReleaseLockAsync(string key, string value)
        {
            try
            {
                if (await GetValueAsync(key) != value)
                    return false;

                return await DeleteKeyAsync(key);
            }
            catch { return false; }
        }
    }
}
