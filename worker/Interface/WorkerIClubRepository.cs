using System.Linq.Expressions;
using worker.Models;

namespace worker.Interfaces
{
    public interface IClubRepository
    {
        Task<Club?> GetByIdAsync(int id);

        Task<List<Club>> FetchBatchAsync(
            int page,
            int pageSize);

        Task<List<Club>> SearchAsync(
            string? search,
            int page,
            int pageSize);

        Task<List<Club>> FetchByConditionAsync(
            Expression<Func<Club, bool>> predicate);
    }
}
