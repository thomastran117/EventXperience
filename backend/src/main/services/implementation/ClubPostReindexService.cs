using backend.main.models.documents;
using backend.main.repositories.interfaces;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;

namespace backend.main.services.implementation
{
    public class ClubPostReindexService : IClubPostReindexService
    {
        private const int BatchSize = 100;

        private readonly IClubPostRepository _postRepository;
        private readonly IClubPostSearchService _searchService;

        public ClubPostReindexService(IClubPostRepository postRepository, IClubPostSearchService searchService)
        {
            _postRepository = postRepository;
            _searchService = searchService;
        }

        public async Task<int> ReindexAllAsync()
        {
            await _searchService.DeleteIndexAsync();
            await _searchService.EnsureIndexAsync();

            int totalIndexed = 0;
            int page = 1;

            while (true)
            {
                var posts = await _postRepository.GetAllForReindexAsync(page, BatchSize);
                if (posts.Count == 0) break;

                var documents = posts.Select(p => new ClubPostDocument
                {
                    Id = p.Id,
                    ClubId = p.ClubId,
                    UserId = p.UserId,
                    Title = p.Title,
                    Content = p.Content,
                    PostType = p.PostType.ToString(),
                    LikesCount = p.LikesCount,
                    IsPinned = p.IsPinned,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                await _searchService.BulkIndexAsync(documents);
                totalIndexed += posts.Count;
                page++;

                if (posts.Count < BatchSize) break;
            }

            Logger.Info($"Reindex complete. {totalIndexed} posts indexed.");
            return totalIndexed;
        }
    }
}
