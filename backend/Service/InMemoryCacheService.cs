using System.Collections.Concurrent;

using backend.Interfaces;

using StackExchange.Redis;

namespace backend.Services
{
    public sealed class InMemoryCacheService : ICacheService
    {
        private sealed record CacheEntry(object Value, DateTime? Expiry);

        private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
        private bool IsExpired(CacheEntry entry) =>
    entry.Expiry is not null && entry.Expiry <= DateTime.UtcNow;

        private bool TryGet(string key, out CacheEntry entry)
        {
            if (_store.TryGetValue(key, out entry!))
            {
                if (IsExpired(entry))
                {
                    _store.TryRemove(key, out _);
                    entry = null!;
                    return false;
                }
                return true;
            }
            return false;
        }

        public Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null)
        {
            _store[key] = new CacheEntry(
                value,
                expiry is null ? null : DateTime.UtcNow.Add(expiry.Value)
            );
            return Task.FromResult(true);
        }

        public Task<string?> GetValueAsync(string key) =>
            Task.FromResult(
                TryGet(key, out var e) ? e.Value as string : null
            );

        public Task<long> IncrementAsync(string key, long value = 1)
        {
            var current = TryGet(key, out var e) && long.TryParse(e.Value.ToString(), out var v)
                ? v
                : 0;

            var next = current + value;
            _store[key] = new CacheEntry(next.ToString(), e?.Expiry);
            return Task.FromResult(next);
        }

        public Task<long> DecrementAsync(string key, long value = 1) =>
            IncrementAsync(key, -value);

        private ConcurrentDictionary<string, string> GetHash(string key) =>
            (TryGet(key, out var e) ? e.Value : null) as ConcurrentDictionary<string, string>
            ?? new ConcurrentDictionary<string, string>();

        public Task<bool> HashSetAsync(string key, string field, string value)
        {
            var hash = GetHash(key);
            hash[field] = value;
            _store[key] = new CacheEntry(hash, null);
            return Task.FromResult(true);
        }

        public Task<string?> HashGetAsync(string key, string field)
        {
            var hash = GetHash(key);
            return Task.FromResult(hash.TryGetValue(field, out var v) ? v : null);
        }

        public Task<Dictionary<string, string>> HashGetAllAsync(string key) =>
            Task.FromResult(new Dictionary<string, string>(GetHash(key)));

        public Task<bool> HashDeleteAsync(string key, string field)
        {
            var hash = GetHash(key);
            return Task.FromResult(hash.TryRemove(field, out _));
        }

        private ConcurrentDictionary<string, byte> GetSet(string key) =>
            (TryGet(key, out var e) ? e.Value : null) as ConcurrentDictionary<string, byte>
            ?? new ConcurrentDictionary<string, byte>();

        public Task<bool> SetAddAsync(string key, string value)
        {
            var set = GetSet(key);
            set.TryAdd(value, 0);
            _store[key] = new CacheEntry(set, null);
            return Task.FromResult(true);
        }

        public Task<bool> SetRemoveAsync(string key, string value)
        {
            var set = GetSet(key);
            return Task.FromResult(set.TryRemove(value, out _));
        }

        public Task<string[]> SetMembersAsync(string key)
        {
            var set = GetSet(key);
            return Task.FromResult(set.Keys.ToArray());
        }

        private ConcurrentQueue<string> GetList(string key) =>
            (TryGet(key, out var e) ? e.Value : null) as ConcurrentQueue<string>
            ?? new ConcurrentQueue<string>();

        public Task<long> ListLeftPushAsync(string key, string value)
        {
            var list = GetList(key);
            list.Enqueue(value);
            _store[key] = new CacheEntry(list, null);
            return Task.FromResult((long)list.Count);
        }

        public Task<long> ListRightPushAsync(string key, string value) =>
            ListLeftPushAsync(key, value);

        public Task<string?> ListLeftPopAsync(string key)
        {
            var list = GetList(key);
            return Task.FromResult(list.TryDequeue(out var v) ? v : null);
        }

        public Task<string?> ListRightPopAsync(string key) =>
            ListLeftPopAsync(key);

        public Task<bool> DeleteKeyAsync(string key) =>
            Task.FromResult(_store.TryRemove(key, out _));

        public Task<bool> KeyExistsAsync(string key) =>
            Task.FromResult(TryGet(key, out _));

        public Task<TimeSpan?> GetTTLAsync(string key) =>
            Task.FromResult(
                TryGet(key, out var e) && e.Expiry is not null
                    ? e.Expiry - DateTime.UtcNow
                    : null
            );

        public Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        {
            if (!TryGet(key, out var e)) return Task.FromResult(false);
            _store[key] = e with { Expiry = DateTime.UtcNow.Add(expiry) };
            return Task.FromResult(true);
        }

        public IEnumerable<string> ScanKeys(IServer _, string pattern) =>
            _store.Keys.Where(k => k.Contains(pattern.Replace("*", "")));

        public Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry)
        {
            return SetValueAsync(key, value, expiry);
        }

        public Task<bool> ReleaseLockAsync(string key, string value)
        {
            var existing = GetValueAsync(key).Result;
            if (existing != value) return Task.FromResult(false);
            return DeleteKeyAsync(key);
        }
    }
}
