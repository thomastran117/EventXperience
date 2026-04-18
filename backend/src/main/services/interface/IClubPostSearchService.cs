using backend.main.models.documents;
using backend.main.models.enums;

namespace backend.main.services.interfaces
{
    public interface IClubPostSearchService
    {
        Task EnsureIndexAsync();
        Task DeleteIndexAsync();
        Task IndexAsync(ClubPostDocument document);
        Task DeleteAsync(int postId);
        Task BulkIndexAsync(IEnumerable<ClubPostDocument> documents);
        Task<(List<int> Ids, int TotalCount)> SearchByClubAsync(int clubId, string search, PostSortBy sortBy, int page, int pageSize);
        Task<(List<int> Ids, int TotalCount)> SearchAllAsync(string search, PostSortBy sortBy, int page, int pageSize);
    }
}
