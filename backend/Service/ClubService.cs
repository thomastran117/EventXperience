using System.Text.Json;

using backend.Common;
using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Mappers;
using backend.Models;
using backend.Utilities;

namespace backend.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _clubRepository;
        private readonly IUserService _userService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ICacheService _cache;

        private static readonly TimeSpan ClubTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClubListTTL = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan NotFoundTTL = TimeSpan.FromSeconds(15);
        private const int HotThreshold = 5;
        private static readonly TimeSpan HotWindow = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan HotCooldown = TimeSpan.FromMinutes(2);
        private const string ClubListVersionKey = "clubs:version";
        private const string NullSentinel = "__null__";

        public ClubService(
            IClubRepository clubRepository,
            IUserService userService,
            IFileUploadService fileUploadService,
            ICacheService cache)
        {
            _clubRepository = clubRepository;
            _userService = userService;
            _fileUploadService = fileUploadService;
            _cache = cache;
        }

        public async Task<Club> CreateClub(
            string name,
            int userId,
            string description,
            string clubtype,
            IFormFile clubimage,
            string? phone = null,
            string? email = null)
        {
            var user = await _userService.GetUserByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var imageUrl = await _fileUploadService.UploadImageAsync(clubimage, "clubs")
                ?? throw new InternalServerException("Image upload failed");

            var club = new Club
            {
                Name = name,
                Description = description,
                Clubtype = Enum.Parse<ClubType>(clubtype, true),
                ClubImage = imageUrl,
                Phone = phone,
                Email = email,
                UserId = userId,
                MemberCount = 0,
            };

            var created = await _clubRepository.CreateAsync(club);

            await CacheClubAsync(created);
            await BumpClubListVersionAsync();

            return created;
        }

        public async Task<Club> GetClub(int clubId)
        {
            var key = $"club:{clubId}";

            var cached = await _cache.GetValueAsync(key);

            if (cached == NullSentinel)
                throw new NotFoundException($"Club {clubId} not found");

            Club club;

            if (cached != null)
            {
                var dto = JsonSerializer.Deserialize<ClubCacheDto>(cached)!;
                club = ClubCacheMapper.ToEntity(dto);
            }
            else
            {
                var fetchedClub = await _clubRepository.GetByIdAsync(clubId);
                if (fetchedClub == null)
                {
                    await _cache.SetValueAsync(key, NullSentinel, NotFoundTTL);
                    throw new NotFoundException($"Club {clubId} not found");
                }

                club = fetchedClub;
                await CacheClubAsync(club);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await TrackClubAccessAsync(clubId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(
                        ex,
                        "Failed to track club access for club"
                    );
                }
            });

            return club;
        }


        public async Task<Club> GetClubByUser(int userId)
        {
            var key = $"club:{userId}";

            var cached = await _cache.GetValueAsync(key);

            if (cached == NullSentinel)
                throw new NotFoundException($"Club {userId} not found");

            Club club;

            if (cached != null)
            {
                var dto = JsonSerializer.Deserialize<ClubCacheDto>(cached)!;
                club = ClubCacheMapper.ToEntity(dto);
            }
            else
            {
                var fetchedClub = await _clubRepository.GetByIdAsync(userId);
                if (fetchedClub == null)
                {
                    await _cache.SetValueAsync(key, NullSentinel, NotFoundTTL);
                    throw new NotFoundException($"Club {userId} not found");
                }

                club = fetchedClub;
                await CacheClubAsync(club);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await TrackClubAccessAsync(userId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(
                        ex,
                        "Failed to track club access for club"
                    );
                }
            });

            return club;
        }

        public async Task<List<Club>> GetAllClubs(
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            var version = await GetClubListVersionAsync();

            var normalizedSearch = search?.Trim().ToLowerInvariant();

            var key = normalizedSearch == null
                ? $"clubs:list:v{version}:p{page}:s{pageSize}"
                : $"clubs:list:v{version}:q:{normalizedSearch}:p{page}:s{pageSize}";

            var cached = await _cache.GetValueAsync(key);
            if (cached != null)
            {
                var dtos = JsonSerializer.Deserialize<List<ClubCacheDto>>(cached)!;
                return dtos.Select(ClubCacheMapper.ToEntity).ToList();
            }

            var clubs = await _clubRepository.SearchAsync(
                normalizedSearch,
                page,
                pageSize
            );

            var dtoList = clubs.Select(ClubCacheMapper.ToDto).ToList();

            await _cache.SetValueAsync(
                key,
                JsonSerializer.Serialize(dtoList),
                WithJitter(ClubListTTL)
            );

            return clubs;
        }

        public async Task<Club> UpdateClub(
            int clubId,
            int userId,
            string name,
            string description,
            string clubtype,
            IFormFile clubimage,
            string? phone = null,
            string? email = null)
        {
            var existing = await GetClub(clubId);

            var newImage = await _fileUploadService.UploadImageAsync(clubimage, "clubs")
                ?? throw new InternalServerException("Image upload failed");

            var updated = await _clubRepository.UpdateAsync(clubId, new Club
            {
                Name = name,
                Description = description,
                Clubtype = Enum.Parse<ClubType>(clubtype, true),
                ClubImage = newImage,
                Phone = phone,
                Email = email,
                MemberCount = existing.MemberCount,
                UserId = userId,
            }) ?? throw new InternalServerException("Update failed");

            await CacheClubAsync(updated);
            await BumpClubListVersionAsync();

            if (!string.IsNullOrWhiteSpace(existing.ClubImage))
                _ = _fileUploadService.DeleteImageAsync(existing.ClubImage);

            return updated;
        }

        public async Task DeleteClub(int clubId, int userId)
        {
            var club = await GetClub(clubId);

            if (club.UserId != userId)
                throw new ForbiddenException("Not allowed");

            await _fileUploadService.DeleteImageAsync(club.ClubImage);

            if (!await _clubRepository.DeleteAsync(clubId))
                throw new InternalServerException("Delete failed");

            await _cache.DeleteKeyAsync($"club:{clubId}");
            await BumpClubListVersionAsync();
        }

        private async Task CacheClubAsync(Club club)
        {
            await _cache.SetValueAsync(
                $"club:{club.Id}",
                JsonSerializer.Serialize(ClubCacheMapper.ToDto(club)),
                WithJitter(ClubTTL)
            );
        }

        private async Task<long> GetClubListVersionAsync()
        {
            var v = await _cache.GetValueAsync(ClubListVersionKey);

            if (v == null)
            {
                await _cache.SetValueAsync(ClubListVersionKey, "1");
                return 1;
            }

            return long.Parse(v);
        }

        private async Task BumpClubListVersionAsync()
        {
            await _cache.IncrementAsync(ClubListVersionKey);
        }

        private static TimeSpan WithJitter(TimeSpan baseTtl, int percent = 20)
        {
            var delta = Random.Shared.Next(-percent, percent + 1);
            return baseTtl + TimeSpan.FromMilliseconds(
                baseTtl.TotalMilliseconds * delta / 100.0
            );
        }

        private async Task TrackClubAccessAsync(int clubId)
        {
            var counterKey = $"club:hot:count:{clubId}";
            var hotFlagKey = $"club:hot:{clubId}";

            if (await _cache.KeyExistsAsync(hotFlagKey))
                return;

            var count = await _cache.IncrementAsync(counterKey);

            if (count == 1)
            {
                await _cache.SetExpiryAsync(counterKey, HotWindow);
            }

            if (count >= HotThreshold)
            {
                await _cache.SetValueAsync(
                    hotFlagKey,
                    "1",
                    HotCooldown
                );

                Console.WriteLine("hot");
            }
        }
    }
}
