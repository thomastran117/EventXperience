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

        public ClubService(
            IClubRepository clubRepository,
            IUserService userService,
            IFileUploadService fileUploadService)
        {
            _userService = userService;
            _clubRepository = clubRepository;
            _fileUploadService = fileUploadService;
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
                ?? throw new NotFoundException();

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

            return await _clubRepository.CreateAsync(club);
        }

        public async Task DeleteClub(int clubId, int userId)
        {
            var club = await _clubRepository.GetByIdAsync(clubId)
                ?? throw new NotFoundException($"Club with id {clubId} is not found");

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to delete club with id {clubId}");

            await _fileUploadService.DeleteImageAsync(club.ClubImage);

            var success = await _clubRepository.DeleteAsync(clubId);
            if (!success)
                throw new InternalServerException("Failed to delete club");
        }

        public async Task<List<Club>> GetAllClubs(
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var clubs = await _clubRepository.GetAllAsync(page, pageSize);

            if (!string.IsNullOrWhiteSpace(search))
            {
                return clubs
                    .Where(c =>
                        c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return clubs.ToList();
        }


        public async Task<Club> GetClub(int clubId)
        {
            return await _clubRepository.GetByIdAsync(clubId)
                ?? throw new NotFoundException($"Club with id {clubId} is not found");
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
            var club = await _clubRepository.GetByIdAsync(clubId)
                ?? throw new NotFoundException($"Club with id {clubId} is not found");

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to update club with id {clubId}");

            var user = await _userService.GetUserByIdAsync(userId)
                ?? throw new NotFoundException();

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
            });

            if (!string.IsNullOrWhiteSpace(oldImageUrl) &&
                !oldImageUrl.Equals(newImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                _ = _fileUploadService.DeleteImageAsync(oldImageUrl);
            }

            return updated!;
        }
    }
}
