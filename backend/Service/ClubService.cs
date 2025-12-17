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

        private const string ClubListVersionKey = "clubs:version";

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
                IsVerified = false,
                User = user
            };

            var created = await _clubRepository.CreateAsync(club);

            await CacheClubAsync(created);
            await BumpClubListVersionAsync();

            return created;
        }

        public async Task<Club> GetClub(int clubId)
        {
            var key = $"club:{clubId}";

            var dto = await CacheHelpers.GetOrSetAsync(
                _cache,
                key,
                async () =>
                {
                    var club = await _clubRepository.GetByIdAsync(clubId)
                        ?? throw new NotFoundException($"Club {clubId} not found");

                    return ClubCacheMapper.ToDto(club);
                },
                ClubTTL
            );

            return dto is null
                ? throw new InternalServerException()
                : ClubCacheMapper.ToEntity(dto);
        }

        public async Task<List<Club>> GetAllClubs(
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            var version = await GetClubListVersionAsync();

            var key = string.IsNullOrWhiteSpace(search)
                ? $"clubs:list:v{version}:page:{page}:size:{pageSize}"
                : $"clubs:list:v{version}:search:{search.ToLower()}:page:{page}:size:{pageSize}";

            var dtos = await CacheHelpers.GetOrSetAsync(
                _cache,
                key,
                async () =>
                {
                    var clubs = await _clubRepository.GetAllAsync(page, pageSize);

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        clubs = clubs.Where(c =>
                            c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            c.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                        ).ToList();
                    }

                    return clubs.Select(ClubCacheMapper.ToDto).ToList();
                },
                ClubListTTL
            );

            return dtos?.Select(ClubCacheMapper.ToEntity).ToList() ?? [];
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

            if (existing.UserId != userId)
                throw new ForbiddenException("Not allowed");

            var newImage = await _fileUploadService.UploadImageAsync(clubimage, "clubs");

            var updated = await _clubRepository.UpdateAsync(clubId, new Club
            {
                Name = name,
                Description = description,
                Clubtype = Enum.Parse<ClubType>(clubtype, true),
                ClubImage = newImage!,
                Phone = phone,
                Email = email,
                MemberCount = existing.MemberCount,
                IsVerified = existing.IsVerified,
                UserId = userId,
                User = existing.User
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
                ClubTTL
            );
        }

        private async Task<long> GetClubListVersionAsync()
        {
            var v = await _cache.GetValueAsync(ClubListVersionKey);
            return long.TryParse(v, out var version) ? version : 1;
        }

        private async Task BumpClubListVersionAsync()
        {
            await _cache.IncrementAsync(ClubListVersionKey);
        }
    }
}
