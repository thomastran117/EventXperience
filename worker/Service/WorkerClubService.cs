using System.Text.Json;

using worker.Interfaces;
using worker.Mappers;
using worker.Models;
using worker.Utilities;

namespace worker.Services
{
    public class ClubCacheService : IClubService
    {
        private readonly IClubRepository _clubRepository;
        private readonly ICacheService _cache;

        private static readonly TimeSpan ClubTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClubListTTL = TimeSpan.FromSeconds(60);

        private const string ClubListVersionKey = "clubs:version";

        public ClubCacheService(
            IClubRepository clubRepository,
            ICacheService cache)
        {
            _clubRepository = clubRepository;
            _cache = cache;
        }

        // ----------------------------------------------------
        // Revalidate individual club entries
        // ----------------------------------------------------
        public async Task RevalidateClubCacheAsync(int batchSize = 100)
        {
            var page = 1;

            while (true)
            {
                var clubs = await _clubRepository.FetchBatchAsync(page, batchSize);
                if (clubs.Count == 0) break;

                foreach (var club in clubs)
                {
                    await _cache.SetValueAsync(
                        CacheKeys.Club(club.Id),
                        JsonSerializer.Serialize(
                            ClubCacheMapper.ToDto(club)
                        ),
                        WithJitter(ClubTTL)
                    );
                }

                page++;
            }
        }

        // ----------------------------------------------------
        // Revalidate club list caches
        // ----------------------------------------------------
        public async Task RevalidateClubListsAsync(
            int[] pages,
            int pageSize = 20)
        {
            var version = await GetVersionAsync();

            foreach (var page in pages)
            {
                var clubs = await _clubRepository.SearchAsync(
                    search: null,
                    page: page,
                    pageSize: pageSize);

                await _cache.SetValueAsync(
                    CacheKeys.ClubList(
                        version,
                        null,
                        page,
                        pageSize),
                    JsonSerializer.Serialize(
                        clubs.Select(ClubCacheMapper.ToDto)
                    ),
                    WithJitter(ClubListTTL)
                );
            }
        }

        // ----------------------------------------------------
        // Full revalidation entrypoint
        // ----------------------------------------------------
        public async Task RevalidateAllAsync()
        {
            await RevalidateClubCacheAsync();
            await RevalidateClubListsAsync(pages: new[] { 1, 2, 3 });
        }

        // ----------------------------------------------------
        // Helpers
        // ----------------------------------------------------
        private async Task<long> GetVersionAsync()
        {
            var v = await _cache.GetValueAsync(ClubListVersionKey);
            return long.TryParse(v, out var n) ? n : 1;
        }

        private static TimeSpan WithJitter(TimeSpan baseTtl, int percent = 20)
        {
            var delta = Random.Shared.Next(-percent, percent + 1);
            return baseTtl + TimeSpan.FromMilliseconds(
                baseTtl.TotalMilliseconds * delta / 100.0
            );
        }
    }
}
