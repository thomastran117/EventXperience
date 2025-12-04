using backend.Interfaces;
using backend.Resources;

using StackExchange.Redis;

namespace backend.Services
{
    public class CacheService : BaseCacheService, ICacheService
    {
        public CacheService(RedisResource redisResource)
            : base(redisResource.Database) { }

        public Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null) =>
            ExecuteAsync(async () => await _db.StringSetAsync(key, value, expiry), fallback: false);

        public Task<string?> GetValueAsync(string key) =>
            ExecuteAsync(async () =>
            {
                var result = await _db.StringGetAsync(key);
                return result.HasValue ? result.ToString() : null;
            });

        public Task<long> IncrementAsync(string key, long value = 1) =>
            ExecuteAsync(async () => await _db.StringIncrementAsync(key, value), fallback: 0);

        public Task<long> DecrementAsync(string key, long value = 1) =>
            ExecuteAsync(async () => await _db.StringDecrementAsync(key, value), fallback: 0);

        public Task<bool> HashSetAsync(string key, string field, string value) =>
            ExecuteAsync(async () => await _db.HashSetAsync(key, field, value), fallback: false);

        public Task<string?> HashGetAsync(string key, string field) =>
            ExecuteAsync(async () =>
            {
                var v = await _db.HashGetAsync(key, field);
                return v.HasValue ? v.ToString() : null;
            });

        public Task<Dictionary<string, string>> HashGetAllAsync(string key) =>
            ExecuteAsync(async () =>
            {
                var entries = await _db.HashGetAllAsync(key);
                return entries.ToDictionary(
                    x => x.Name.ToString(),
                    x => x.Value.ToString()
                );
            }, fallback: new Dictionary<string, string>());

        public Task<bool> HashDeleteAsync(string key, string field) =>
            ExecuteAsync(async () => await _db.HashDeleteAsync(key, field), fallback: false);

        public Task<bool> SetAddAsync(string key, string value) =>
            ExecuteAsync(async () => await _db.SetAddAsync(key, value), fallback: false);

        public Task<bool> SetRemoveAsync(string key, string value) =>
            ExecuteAsync(async () => await _db.SetRemoveAsync(key, value), fallback: false);

        public Task<string[]> SetMembersAsync(string key) =>
            ExecuteAsync(async () =>
            {
                var members = await _db.SetMembersAsync(key);
                return members.Select(m => m.ToString()).ToArray();
            }, fallback: Array.Empty<string>());

        public Task<long> ListLeftPushAsync(string key, string value) =>
            ExecuteAsync(async () => await _db.ListLeftPushAsync(key, value), fallback: 0);

        public Task<long> ListRightPushAsync(string key, string value) =>
            ExecuteAsync(async () => await _db.ListRightPushAsync(key, value), fallback: 0);

        public Task<string?> ListLeftPopAsync(string key) =>
            ExecuteAsync(async () =>
            {
                var v = await _db.ListLeftPopAsync(key);
                return v.HasValue ? v.ToString() : null;
            });

        public Task<string?> ListRightPopAsync(string key) =>
            ExecuteAsync(async () =>
            {
                var v = await _db.ListRightPopAsync(key);
                return v.HasValue ? v.ToString() : null;
            });

        public Task<bool> DeleteKeyAsync(string key) =>
            ExecuteAsync(async () => await _db.KeyDeleteAsync(key), fallback: false);

        public Task<bool> KeyExistsAsync(string key) =>
            ExecuteAsync(async () => await _db.KeyExistsAsync(key), fallback: false);

        public Task<TimeSpan?> GetTTLAsync(string key) =>
            ExecuteAsync(async () => await _db.KeyTimeToLiveAsync(key));

        public Task<bool> SetExpiryAsync(string key, TimeSpan expiry) =>
            ExecuteAsync(async () => await _db.KeyExpireAsync(key, expiry), fallback: false);

        public IEnumerable<string> ScanKeys(IServer server, string pattern)
        {
            foreach (var key in server.Keys(pattern: pattern))
                yield return key.ToString();
        }

        public Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry) =>
            ExecuteAsync(async () =>
                await _db.StringSetAsync(key, value, expiry, when: When.NotExists),
                fallback: false);

        public Task<bool> ReleaseLockAsync(string key, string value) =>
            ExecuteAsync(async () =>
            {
                var existing = await GetValueAsync(key);
                if (existing != value)
                    return false;

                return await DeleteKeyAsync(key);
            }, fallback: false);
    }
}
