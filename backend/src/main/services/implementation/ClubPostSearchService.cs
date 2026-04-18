using backend.main.configurations.resource.elasticsearch;
using backend.main.models.documents;
using backend.main.models.enums;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace backend.main.services.implementation
{
    public class ClubPostSearchService : IClubPostSearchService
    {
        private const string IndexName = "club_posts";

        private readonly ElasticsearchClient? _client;
        private readonly ElasticsearchHealth _health;

        public ClubPostSearchService(ElasticsearchHealth health, ElasticsearchClient? client = null)
        {
            _health = health;
            _client = client;
        }

        public async Task EnsureIndexAsync()
        {
            if (!_health.IsAvailable || _client == null) return;

            var exists = await _client.Indices.ExistsAsync(IndexName);
            if (exists.Exists) return;

            await _client.Indices.CreateAsync(IndexName, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(1)
                )
                .Mappings(m => m
                    .Properties<ClubPostDocument>(p => p
                        .IntegerNumber(f => f.Id)
                        .IntegerNumber(f => f.ClubId)
                        .IntegerNumber(f => f.UserId)
                        .Text(f => f.Title, t => t
                            .Analyzer("english")
                            .Fields(ff => ff.Keyword("keyword", k => k.IgnoreAbove(256)))
                        )
                        .Text(f => f.Content, t => t.Analyzer("english"))
                        .Keyword(f => f.PostType)
                        .IntegerNumber(f => f.LikesCount)
                        .Boolean(f => f.IsPinned)
                        .Date(f => f.CreatedAt)
                        .Date(f => f.UpdatedAt)
                    )
                )
            );

            Logger.Info("Elasticsearch index 'club_posts' created.");
        }

        public async Task DeleteIndexAsync()
        {
            if (!_health.IsAvailable || _client == null) return;
            await _client.Indices.DeleteAsync(IndexName);
        }

        public async Task IndexAsync(ClubPostDocument document)
        {
            if (!_health.IsAvailable || _client == null) return;
            await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.Id));
        }

        public async Task DeleteAsync(int postId)
        {
            if (!_health.IsAvailable || _client == null) return;
            await _client.DeleteAsync(IndexName, postId);
        }

        public async Task BulkIndexAsync(IEnumerable<ClubPostDocument> documents)
        {
            if (!_health.IsAvailable || _client == null) return;

            var response = await _client.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(documents)
            );

            if (response.Errors)
                Logger.Warn($"Bulk index had errors: {response.ItemsWithErrors.Count()} items failed.");
        }

        public async Task<(List<int> Ids, int TotalCount)> SearchByClubAsync(
            int clubId, string search, PostSortBy sortBy, int page, int pageSize)
        {
            if (!_health.IsAvailable || _client == null)
                throw new InvalidOperationException("Elasticsearch is not available.");

            var from = (page - 1) * pageSize;

            var response = await _client.SearchAsync<ClubPostDocument>(s => s
                .Index(IndexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Term(t => t.Field(d => d.ClubId).Value(clubId)))
                        .Must(m => m.MultiMatch(mm => mm
                            .Query(search)
                            .Fields((Fields)new Field[] { (Field)"title^3", (Field)"content" })
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(new Fuzziness("AUTO"))
                        ))
                    )
                )
                .Sort(BuildSort(sortBy))
            );

            var ids = response.Hits
                .Where(h => h.Source != null)
                .Select(h => h.Source!.Id)
                .ToList();
            return (ids, (int)response.Total);
        }

        public async Task<(List<int> Ids, int TotalCount)> SearchAllAsync(
            string search, PostSortBy sortBy, int page, int pageSize)
        {
            if (!_health.IsAvailable || _client == null)
                throw new InvalidOperationException("Elasticsearch is not available.");

            var from = (page - 1) * pageSize;

            var response = await _client.SearchAsync<ClubPostDocument>(s => s
                .Index(IndexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(search)
                        .Fields((Fields)new Field[] { (Field)"title^3", (Field)"content" })
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                )
                .Sort(BuildSort(sortBy))
            );

            var ids = response.Hits
                .Where(h => h.Source != null)
                .Select(h => h.Source!.Id)
                .ToList();
            return (ids, (int)response.Total);
        }

        private static Action<SortOptionsDescriptor<ClubPostDocument>> BuildSort(PostSortBy sortBy)
        {
            return sortBy == PostSortBy.Popular
                ? s => s
                    .Field(f => f.IsPinned, fs => fs.Order(SortOrder.Desc))
                    .Field(f => f.LikesCount, fs => fs.Order(SortOrder.Desc))
                : s => s
                    .Field(f => f.IsPinned, fs => fs.Order(SortOrder.Desc))
                    .Field(f => f.CreatedAt, fs => fs.Order(SortOrder.Desc));
        }
    }
}
