using System.Text.Json;

using backend.Common;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;

namespace backend.Services
{
    public class ClubService : IClubService
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IClubRepository _clubRepository;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        private static readonly TimeSpan ClubCacheTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClubListCacheTTL = TimeSpan.FromSeconds(60);

        public ClubService(
            IClubRepository clubRepository,
            IUserService userService,
            IFileUploadService fileUploadService,
            ICacheService cacheService)
        {
            _userService = userService;
            _clubRepository = clubRepository;
            _fileUploadService = fileUploadService;
            _cacheService = cacheService;
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

            var imageUrl = await _fileUploadService
                .UploadImageAsync(clubimage, "clubs")
                ?? throw new InternalServerException("Failed to upload club image");

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
            await InvalidateClubListsAsync();

            return created;
        }
        public async Task DeleteClub(int clubId, int userId)
        {
            var cacheKey = GetClubCacheKey(clubId);

            var club = await GetClub(clubId);

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to delete club with id {clubId}");

            await _fileUploadService.DeleteImageAsync(club.ClubImage);

            var success = await _clubRepository.DeleteAsync(clubId);
            if (!success)
                throw new InternalServerException("Failed to delete club");

            await _cacheService.DeleteKeyAsync(cacheKey);
            await InvalidateClubListsAsync();
        }

        public async Task<List<Club>> GetAllClubs(
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            string cacheKey = GetClubListCacheKey(search, page, pageSize);

            var cached = await _cacheService.GetValueAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return JsonSerializer.Deserialize<List<Club>>(cached)!;
            }

            var clubs = await _clubRepository.GetAllAsync(page, pageSize);

            if (!string.IsNullOrWhiteSpace(search))
            {
                clubs = clubs
                    .Where(c =>
                        c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            await _cacheService.SetValueAsync(
                cacheKey,
                JsonSerializer.Serialize(clubs),
                ClubListCacheTTL
            );

            return clubs.ToList();
        }

        public async Task<Club> GetClub(int clubId)
        {
            string cacheKey = GetClubCacheKey(clubId);

            var cached = await _cacheService.GetValueAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return JsonSerializer.Deserialize<Club>(cached)!;
            }

            var club = await _clubRepository.GetByIdAsync(clubId)
                ?? throw new NotFoundException($"Club with id {clubId} is not found");

            await CacheClubAsync(club);

            return club;
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
            var club = await GetClub(clubId);

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to update club with id {clubId}");

            var user = await _userService.GetUserByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var oldImageUrl = club.ClubImage;
            var newImageUrl = await _fileUploadService.UploadImageAsync(clubimage, "clubs");

            var updated = await _clubRepository.UpdateAsync(clubId, new Club
            {
                Name = name,
                Description = description,
                Clubtype = Enum.Parse<ClubType>(clubtype, true),
                ClubImage = newImageUrl!,
                Phone = phone,
                Email = email,
                IsVerified = club.IsVerified,
                MemberCount = club.MemberCount,
                User = user
            }) ?? throw new InternalServerException("Club update failed");

            await CacheClubAsync(updated);
            await InvalidateClubListsAsync();

            if (!string.IsNullOrWhiteSpace(oldImageUrl) &&
                !oldImageUrl.Equals(newImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                _ = _fileUploadService.DeleteImageAsync(oldImageUrl);
            }

            return updated;
        }

        private async Task CacheClubAsync(Club club)
        {
            string key = GetClubCacheKey(club.Id);

            await _cacheService.SetValueAsync(
                key,
                JsonSerializer.Serialize(club),
                ClubCacheTTL
            );
        }

        private static string GetClubCacheKey(int id)
            => $"club:{id}";

        private static string GetClubListCacheKey(string? search, int page, int size)
            => string.IsNullOrWhiteSpace(search)
                ? $"clubs:page:{page}:size:{size}"
                : $"clubs:search:{search.ToLower()}:page:{page}:size:{size}";

        private async Task InvalidateClubListsAsync()
        {
            try
            {

                var server = _cacheService.GetServer();

                foreach (var key in _cacheService.ScanKeys(server, "clubs:*"))
                {
                    await _cacheService.DeleteKeyAsync(key);
                }
            }
            catch
            {
                // skip
            }
        }

    }
}
