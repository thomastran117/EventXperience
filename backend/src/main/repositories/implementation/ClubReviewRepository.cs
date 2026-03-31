using backend.main.configurations.resource.database;
using backend.main.models.core;
using backend.main.repositories.interfaces;

using Microsoft.EntityFrameworkCore;

namespace backend.main.repositories.implementation
{
    public class ClubReviewRepository : BaseRepository, IClubReviewRepository
    {
        public ClubReviewRepository(AppDatabaseContext context) : base(context) { }

        public async Task<ClubReview> CreateAsync(ClubReview review)
        {
            return await ExecuteAsync(async () =>
            {
                _context.ClubReviews.Add(review);
                await _context.SaveChangesAsync();
                return review;
            })!;
        }

        public async Task<List<ClubReview>> GetByClubIdAsync(int clubId, int page, int pageSize)
        {
            return await ExecuteAsync(async () =>
            {
                return await _context.ClubReviews
                    .AsNoTracking()
                    .Where(r => r.ClubId == clubId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            })!;
        }

        public async Task<List<ClubReview>> GetByUserIdAsync(int userId, int page, int pageSize)
        {
            return await ExecuteAsync(async () =>
            {
                return await _context.ClubReviews
                    .AsNoTracking()
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            })!;
        }

        public async Task<ClubReview?> GetByIdAsync(int id)
        {
            return await ExecuteAsync(async () =>
            {
                return await _context.ClubReviews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            });
        }

        public async Task<ClubReview?> UpdateAsync(int id, ClubReview updated)
        {
            return await ExecuteAsync(async () =>
            {
                var existing = await _context.ClubReviews.FindAsync(id);
                if (existing == null)
                    return null;

                existing.Title = updated.Title;
                existing.Rating = updated.Rating;
                existing.Comment = updated.Comment;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existing;
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await ExecuteAsync(async () =>
            {
                var review = await _context.ClubReviews.FindAsync(id);
                if (review == null)
                    return false;

                _context.ClubReviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            })!;
        }

        public async Task<double?> GetAverageRatingAsync(int clubId)
        {
            return await ExecuteAsync(async () =>
            {
                var hasReviews = await _context.ClubReviews.AnyAsync(r => r.ClubId == clubId);
                if (!hasReviews)
                    return (double?)null;

                return await _context.ClubReviews
                    .Where(r => r.ClubId == clubId)
                    .AverageAsync(r => (double)r.Rating);
            });
        }
    }
}
