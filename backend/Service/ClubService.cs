using Microsoft.EntityFrameworkCore;

using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Resources;

namespace backend.Services
{
    public class ClubService : IClubService
    {
        private readonly AppDatabaseContext _context;
        private readonly IFileUploadService _fileUploadService;

        public ClubService(AppDatabaseContext context, IFileUploadService fileUploadService)
        {
            _context = context;
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
                ?? throw new InternalServerException("Internal server error occured when uploading the image");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException($"The user with the ID {userId} is not found");

            var club = new Club
            {
                Name = name,
                Description = description,
                Clubtype = clubtype,
                ClubImage = imageUrl,
                Phone = phone,
                Email = email,
                UserId = userId,
                User = user
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return club;
        }

        public async Task DeleteClub(int clubId, int userId)
        {
            var club = await _context.Clubs
                .FirstOrDefaultAsync(c => c.Id == clubId)
                ?? throw new NotFoundException($"Club with the id {clubId} is not found");

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to delete club with id of {clubId}");

            _ = _fileUploadService.DeleteImageAsync(club.ClubImage);

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return;
        }

        public async Task<List<Club>> GetAllClubs(string? query = null)
        {
            IQueryable<Club> q = _context.Clubs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query.Trim()}%";
                q = q.Where(c =>
                    EF.Functions.ILike(c.Name, like) ||
                    EF.Functions.ILike(c.Description, like) ||
                    EF.Functions.ILike(c.Clubtype, like));
            }

            return await q
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<Club> GetClub(int clubId)
        {
            var club = await _context.Clubs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clubId)
                ?? throw new NotFoundException($"Club with the id {clubId} is not found");

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
            var club = await _context.Clubs
                .FirstOrDefaultAsync(c => c.Id == clubId)
                ?? throw new NotFoundException($"Club with the id {clubId} is not found");

            if (club.UserId != userId)
                throw new ForbiddenException($"You are not allowed to update club with id of {clubId}");

            string? oldImageUrl = club.ClubImage;
            var newImageUrl = await _fileUploadService.UploadImageAsync(clubimage, "clubs");

            club.Name = name;
            club.Description = description;
            club.Clubtype = clubtype;
            club.ClubImage = newImageUrl;
            club.Phone = phone;
            club.Email = email;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldImageUrl) && !oldImageUrl.Equals(newImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                _ = _fileUploadService.DeleteImageAsync(oldImageUrl);
            }

            return club;
        }
    }
}
