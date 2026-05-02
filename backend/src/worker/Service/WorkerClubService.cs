using System.Text.Json;

using worker.Interfaces;
using worker.Mappers;
using worker.Models;
using worker.Utilities;

namespace backend.Services
{
    public class WorkerClubService
    {
        private readonly IClubRepository _clubRepository;
        private readonly ICacheService _cache;
        private static readonly TimeSpan ClubTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClubListTTL = TimeSpan.FromSeconds(60);
        private static string LockKey(string key) => $"lock:{key}";
        private const int LockSeconds = 10;
        private const string ClubListVersionKey = "clubs:version";

        public WorkerClubService(
            IClubRepository clubRepository,
            ICacheService cache)
        {
            _clubRepository = clubRepository;
            _cache = cache;
        }

        public async Task WarmAsync(
            int maxPages = 3,
            int pageSize = 20)
        {
            var version = await EnsureClubListVersionAsync();

            for (var page = 1; page <= maxPages; page++)
            {
                var warmed = await WarmClubListPageAsync(version, page, pageSize);

                if (!warmed)
                    break;
            }

            var clubs = await _clubRepository.SearchAsync(
                search: null,
                page: 1,
                pageSize: maxPages * pageSize
            );

            foreach (var club in clubs)
            {
                await CacheClubAsync(club);
            }
        }

        public async Task WarmClubAndInvalidateListsAsync(int clubId)
        {
            await WarmClubAsync(clubId);
            await _cache.IncrementAsync(ClubListVersionKey);
        }

        public async Task WarmClubAsync(int clubId)
        {
            var key = $"club:{clubId}";
            var ttl = await _cache.GetTTLAsync(key);

            if (!CacheRefreshPolicy.ShouldRefresh(
                    ttl,
                    refreshAhead: TimeSpan.FromSeconds(30)))
            {
                return;
            }

            if (!await TryAcquireLockAsync(key))
                return;

            try
            {
                var club = await _clubRepository.GetByIdAsync(clubId);
                if (club == null)
                    return;

                await _cache.SetValueAsync(
                    key,
                    JsonSerializer.Serialize(ClubCacheMapper.ToDto(club)),
                    WithJitter(ClubTTL)
                );
            }
            finally
            {
                await ReleaseLockAsync(key);
            }
        }

        private async Task<bool> WarmClubListPageAsync(
            long version,
            int page,
            int pageSize)
        {
            var key = $"clubs:list:v{version}:p{page}:s{pageSize}";
            var ttl = await _cache.GetTTLAsync(key);

            if (!CacheRefreshPolicy.ShouldRefresh(
                    ttl,
                    refreshAhead: TimeSpan.FromSeconds(30)))
            {
                return true;
            }

            if (!await TryAcquireLockAsync(key))
                return true;

            try
            {
                var clubs = await _clubRepository.SearchAsync(
                    search: null,
                    page: page,
                    pageSize: pageSize
                );

                if (clubs.Count == 0)
                    return false; // 🛑 stop paging

                var dtoList = clubs
                    .Select(ClubCacheMapper.ToDto)
                    .ToList();

                await _cache.SetValueAsync(
                    key,
                    JsonSerializer.Serialize(dtoList),
                    WithJitter(ClubListTTL)
                );

                return true;
            }
            finally
            {
                await ReleaseLockAsync(key);
            }
        }
        private async Task CacheClubAsync(Club club)
        {
            var key = $"club:{club.Id}";
            var ttl = await _cache.GetTTLAsync(key);

            if (!CacheRefreshPolicy.ShouldRefresh(
                    ttl,
                    refreshAhead: TimeSpan.FromSeconds(30)))
            {
                return;
            }

            await _cache.SetValueAsync(
                key,
                JsonSerializer.Serialize(ClubCacheMapper.ToDto(club)),
                WithJitter(ClubTTL)
            );
        }
        private async Task<long> EnsureClubListVersionAsync()
        {
            var v = await _cache.GetValueAsync(ClubListVersionKey);

            if (v != null && long.TryParse(v, out var parsed))
                return parsed;

            await _cache.SetValueAsync(ClubListVersionKey, "1");
            return 1;
        }

        private static TimeSpan WithJitter(TimeSpan baseTtl, int percent = 20)
        {
            var delta = Random.Shared.Next(-percent, percent + 1);
            return baseTtl + TimeSpan.FromMilliseconds(
                baseTtl.TotalMilliseconds * delta / 100.0
            );
        }

        private async Task<bool> TryAcquireLockAsync(string key)
        {
            return await _cache.AcquireLockAsync(
                LockKey(key),
                Environment.MachineName,
                TimeSpan.FromSeconds(LockSeconds)
            );
        }

        private async Task ReleaseLockAsync(string key)
        {
            await _cache.ReleaseLockAsync(
                LockKey(key),
                Environment.MachineName
            );
        }
    }
}
