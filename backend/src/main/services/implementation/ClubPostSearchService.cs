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
        private readonly SemaphoreSlim _indexLock = new(1, 1);
        private bool _indexEnsured;

        public ClubPostSearchService(ElasticsearchHealth health, ElasticsearchClient? client = null)
        {
            _health = health;
            _client = client;
        }

        public async Task EnsureIndexAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_indexEnsured)
                return;

            var client = GetRequiredClient();

            await _indexLock.WaitAsync(cancellationToken);
            try
            {
                if (_indexEnsured)
                    return;

                var exists = await client.Indices.ExistsAsync(IndexName);
                if (!exists.Exists)
                {
                    await client.Indices.CreateAsync(IndexName, c => c
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

                _indexEnsured = true;
            }
            catch (ElasticsearchServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    "Failed to verify the Elasticsearch club post index.",
                    ex);
            }
            finally
            {
                _indexLock.Release();
            }
        }

        public async Task DeleteIndexAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetWritableClientOrNull();
            if (client == null) return;

            try
            {
                await client.Indices.DeleteAsync(IndexName);
                _indexEnsured = false;
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    "Failed to delete the Elasticsearch club post index.",
                    ex);
            }
        }

        public async Task IndexAsync(ClubPostDocument document, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetWritableClientOrNull();
            if (client == null) return;

            await EnsureIndexAsync(cancellationToken);

            try
            {
                await client.IndexAsync(document, i => i.Index(IndexName).Id(document.Id));
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    $"Failed to index club post document {document.Id}.",
                    ex);
            }
        }

        public async Task DeleteAsync(int postId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetWritableClientOrNull();
            if (client == null) return;

            try
            {
                await client.DeleteAsync(IndexName, postId);
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    $"Failed to delete club post document {postId}.",
                    ex);
            }
        }

        public async Task BulkIndexAsync(IEnumerable<ClubPostDocument> documents, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetWritableClientOrNull();
            if (client == null) return;

            await EnsureIndexAsync(cancellationToken);

            try
            {
                var response = await client.BulkAsync(b => b
                    .Index(IndexName)
                    .IndexMany(documents)
                );

                if (response.Errors)
                    Logger.Warn($"Bulk index had errors: {response.ItemsWithErrors.Count()} items failed.");
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    "Failed to bulk index club post documents.",
                    ex);
            }
        }

        public async Task<(List<int> Ids, int TotalCount)> SearchByClubAsync(
            int clubId, string search, PostSortBy sortBy, int page, int pageSize)
        {
            var client = GetRequiredClient();

            await EnsureIndexAsync();

            var from = (page - 1) * pageSize;

            try
            {
                var response = await client.SearchAsync<ClubPostDocument>(s => s
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
            catch (ElasticsearchServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    "Elasticsearch search failed for club posts.",
                    ex);
            }
        }

        public async Task<(List<int> Ids, int TotalCount)> SearchAllAsync(
            string search, PostSortBy sortBy, int page, int pageSize)
        {
            var client = GetRequiredClient();

            await EnsureIndexAsync();

            var from = (page - 1) * pageSize;

            try
            {
                var response = await client.SearchAsync<ClubPostDocument>(s => s
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
            catch (ElasticsearchServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ElasticsearchUnavailableException(
                    "Elasticsearch search failed for admin club posts.",
                    ex);
            }
        }

        private ElasticsearchClient GetRequiredClient()
        {
            if (_client != null)
                return _client;

            if (!_health.IsConfigured)
                throw new ElasticsearchDisabledException(
                    "Elasticsearch is disabled because ELASTICSEARCH_URL is not configured.");

            if (_health.Failure != null)
                throw new ElasticsearchConfigurationException(
                    "Elasticsearch is configured but failed to initialize.",
                    _health.Failure);

            throw new ElasticsearchUnavailableException("Elasticsearch client is unavailable.");
        }

        private ElasticsearchClient? GetWritableClientOrNull()
        {
            if (_client != null)
                return _client;

            if (!_health.IsConfigured)
                return null;

            if (_health.Failure != null)
                throw new ElasticsearchConfigurationException(
                    "Elasticsearch is configured but failed to initialize.",
                    _health.Failure);

            throw new ElasticsearchUnavailableException("Elasticsearch client is unavailable.");
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
