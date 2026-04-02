using backend.main.configurations.resource.database;
using backend.main.models.core;
using backend.main.repositories.interfaces;

using Microsoft.EntityFrameworkCore;

namespace backend.main.repositories.implementation
{
    public class ClubAnnouncementRepository : IClubAnnouncementRepository
    {
        private readonly AppDatabaseContext _context;

        public ClubAnnouncementRepository(AppDatabaseContext context) => _context = context;

        public async Task<ClubAnnouncement> CreateAsync(ClubAnnouncement announcement)
        {
            _context.ClubAnnouncements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<ClubAnnouncement?> GetByIdAsync(int id)
        {
            return await _context.ClubAnnouncements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ClubAnnouncement>> GetByClubIdAsync(int clubId, string? search, int page, int pageSize)
        {
            IQueryable<ClubAnnouncement> query = _context.ClubAnnouncements
                .AsNoTracking()
                .Where(a => a.ClubId == clubId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Title, $"%{term}%") ||
                    EF.Functions.Like(a.Content, $"%{term}%"));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByClubIdAsync(int clubId, string? search)
        {
            IQueryable<ClubAnnouncement> query = _context.ClubAnnouncements
                .Where(a => a.ClubId == clubId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Title, $"%{term}%") ||
                    EF.Functions.Like(a.Content, $"%{term}%"));
            }

            return await query.CountAsync();
        }

        public async Task<List<ClubAnnouncement>> GetAllAsync(string? search, int page, int pageSize)
        {
            IQueryable<ClubAnnouncement> query = _context.ClubAnnouncements.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Title, $"%{term}%") ||
                    EF.Functions.Like(a.Content, $"%{term}%"));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAllAsync(string? search)
        {
            IQueryable<ClubAnnouncement> query = _context.ClubAnnouncements.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Title, $"%{term}%") ||
                    EF.Functions.Like(a.Content, $"%{term}%"));
            }

            return await query.CountAsync();
        }

        public async Task<ClubAnnouncement?> UpdateAsync(int id, ClubAnnouncement updated)
        {
            var existing = await _context.ClubAnnouncements.FindAsync(id);
            if (existing == null)
                return null;

            existing.Title = updated.Title;
            existing.Content = updated.Content;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var announcement = await _context.ClubAnnouncements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.ClubAnnouncements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
