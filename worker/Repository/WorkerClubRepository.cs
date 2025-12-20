using worker.Interfaces;
using worker.Models;
using worker.Resources;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace worker.Repositories
{
public class ClubRepository : IClubRepository
{
        private readonly WorkerDatabaseContext _context;

        public ClubRepository(WorkerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Club?> GetByIdAsync(int id)
        {
            return await _context.Clubs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Club>> FetchBatchAsync(
            int page,
            int pageSize)
        {
            return await _context.Clubs
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Club>> SearchAsync(
            string? search,
            int page,
            int pageSize)
        {
            IQueryable<Club> query = _context.Clubs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, $"%{search}%") ||
                    EF.Functions.Like(c.Description, $"%{search}%"));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Club>> FetchByConditionAsync(
            Expression<Func<Club, bool>> predicate)
        {
            return await _context.Clubs
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }
    }
}
