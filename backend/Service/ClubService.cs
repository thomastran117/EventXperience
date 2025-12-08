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

        public ClubService(
            IClubRepository clubRepository,
            IFileUploadService fileUploadService)
        {
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
                User = null
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

        public async Task<List<Club>> GetAllClubs(string? query = null)
        {
            var clubs = await _clubRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(query))
            {
                return clubs
                    .Where(c =>
                        c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
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
                User = null
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
