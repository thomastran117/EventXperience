# Event Search Audit: Partial Term Misses and Hard-to-Use API

## Summary

Event search currently misses obvious user queries like `water` or `waterfr` for events containing `Waterfront`, even when indexing is working correctly. The main issue is not document ingestion; it is the search query and field mapping strategy used by Elasticsearch.

The API shape is also difficult to consume because `GET /events` mixes full-text search, faceting, geo filtering, pagination, and sorting into a long list of query parameters with cross-field rules.

## Reproduction

1. Create or reindex events containing `Waterfront` in searchable fields such as `Name`, `Location`, or `VenueName`.
2. Query `GET /events?search=waterfront`.
3. Observe that Elasticsearch returns matches successfully.
4. Query `GET /events?search=waterfr`.
5. Observe that Elasticsearch returns no matches.
6. Query `GET /events?search=water`.
7. Observe that partial-term expectations are not satisfied consistently.

### Observed Elasticsearch example

For `GET /events?search=waterfront`, Elasticsearch returned `source: "elasticsearch"` with `totalCount: 3`, including:

- `Waterfront Recovery Stretch`
- `Yoga for Runners #6` with `location = "Waterfront Studio"` and `venueName = "Waterfront Wellness Studio"`
- `Yoga for Runners #16` with `location = "Waterfront Studio"` and `venueName = "Waterfront Wellness Studio"`

For `GET /events?search=waterfr`, no results were returned.

This is strong evidence that:

- exact token queries like `waterfront` can match
- prefix or substring-style queries like `waterfr` do not
- the current search experience does not behave the way a user would expect from a modern event search box

## Audit Findings

### 1. Indexing appears correct

The event name is present in the indexed document model and is included in the outbox/indexing pipeline.

- `backend/src/main/mappers/EventSearchDocumentMapper.cs`
- `backend/src/main/services/implementation/EventSearchOutboxWriter.cs`
- `backend/src/main/consumers/ElasticsearchIndexMessageValidator.cs`

This means the failure is not caused by `Name` being omitted from the indexed payload.

### 1a. Why the current event search feels suboptimal to users

The current implementation does not match the mental model users have for a search box.

What users expect:

- if an event contains `Waterfront`, then `waterfront`, `waterfr`, and often `water` should all feel like reasonable searches
- obvious fragments of a visible event name or venue should narrow toward that result

What the current Elasticsearch setup actually does:

- it matches analyzed whole terms well
- it tolerates small misspellings reasonably well
- it does not support partial-token discovery unless we explicitly add that capability

That makes search feel broken even when Elasticsearch is technically behaving as configured. In other words, the current setup is optimized for term relevance, but the product experience needs fragment-friendly retrieval.

### 2. The `Name` field is analyzed for whole-word relevance, not partial-term matching

In Elasticsearch, `Name`, `Description`, `Location`, `VenueName`, and `City` use the `english` analyzer.

- `backend/src/main/services/implementation/EventSearchService.cs:64`
- `backend/src/main/services/implementation/EventSearchService.cs:68`
- `backend/src/main/services/implementation/EventSearchService.cs:69`
- `backend/src/main/services/implementation/EventSearchService.cs:71`
- `backend/src/main/services/implementation/EventSearchService.cs:75`

The search query uses `multi_match` with `best_fields` and `AUTO` fuzziness.

- `backend/src/main/services/implementation/EventSearchService.cs:310`

This setup is good for typo tolerance and token-based relevance, but it does not support infix or substring matching inside a token. In practice:

- `waterfront` is indexed as a token like `waterfront`
- `waterfr` is queried as a token like `waterfr`
- `water` is queried as a token like `water`
- `waterfr` does not match the `waterfront` token as a prefix
- `multi_match` does not treat `water` as a substring of `waterfront`
- fuzziness helps with edit distance, not token containment

That is why `waterfr` and `water` do not reliably match `Waterfront`.

### 3. `tags` are included in full-text search, but mapped as `keyword`

The query boosts `tags^2.5` in the free-text search clause:

- `backend/src/main/services/implementation/EventSearchService.cs:317`

But the mapping stores `Tags` as `keyword`:

- `backend/src/main/services/implementation/EventSearchService.cs:79`

That means tag search behavior is effectively exact-value oriented, not partial or analyzed text search. So free-text queries over tags are also less forgiving than they appear from the query code.

### 4. Search semantics differ between Elasticsearch and the database fallback

When Elasticsearch is unavailable, the app falls back to MySQL search:

- `backend/src/main/services/implementation/EventsService.cs:168`
- `backend/src/main/repositories/implementation/EventsRepository.cs:106`

The fallback uses SQL `LIKE '%term%'` against text fields:

- `backend/src/main/repositories/implementation/EventsRepository.cs:197`

That fallback *does* support substring matching such as `water` -> `Waterfront` and `waterfr` -> `Waterfront`.

Result: behavior changes depending on whether the response source is Elasticsearch or database. That makes search feel inconsistent and harder to debug.

### 5. The event search API is doing too much through query-string fields

`GET /events` currently accepts:

- `search`
- `isPrivate`
- `status`
- `category`
- `tags`
- `city`
- `lat`
- `lng`
- `radiusKm`
- `sortBy`
- `page`
- `pageSize`

Code reference:

- `backend/src/main/controllers/implementation/EventsController.cs:199`

The controller also contains cross-field validation and custom parsing rules:

- `lat` and `lng` must be paired
- `sortBy=Distance` requires `lat` and `lng`
- `tags` is a comma-delimited string that is normalized manually

Code reference:

- `backend/src/main/controllers/implementation/EventsController.cs:222`
- `backend/src/main/controllers/implementation/EventsController.cs:228`
- `backend/src/main/controllers/implementation/EventsController.cs:231`

This is workable for manual testing, but awkward for API consumers and likely to get worse as search evolves.

### 6. There is no test coverage for the failing search behavior

Current test coverage touches indexing worker behavior, but not event search semantics such as:

- partial-token matches
- exact-vs-fuzzy text behavior
- Elasticsearch vs database fallback consistency
- API parsing/validation for combined search filters

### 7. Club search does not appear to suffer from the same issue

Club search is currently implemented through the database rather than Elasticsearch.

The clubs endpoint is a simpler `GET /clubs?search=...` flow:

- `backend/src/main/controllers/implementation/ClubController.cs:146`
- `backend/src/main/services/implementation/ClubService.cs:174`
- `backend/src/main/repositories/implementation/ClubRepository.cs:97`

The repository uses SQL substring matching:

- `backend/src/main/repositories/implementation/ClubRepository.cs:109`

Specifically, club search applies:

- `LIKE '%term%'` on `Name`
- `LIKE '%term%'` on `Description`

That means clubs should already support fragment-style matches like:

- `water` -> `Waterfront Running Club`
- `waterfr` -> `Waterfront Running Club`

So based on the current code audit:

- event search has a partial-token gap on the Elasticsearch path
- club search does not appear to have the same gap because it uses database substring matching

I did not find separate club-search tests confirming this behavior, so there is still value in adding a small regression test there, but it does not currently look like a second issue of the same class.

## Root Cause

The current search implementation is optimized for analyzed full-text relevance, not user-friendly partial matching.

Specifically:

1. Searchable text fields use `english` analysis only.
2. The query uses `multi_match`, which matches analyzed terms rather than substrings.
3. There is no dedicated subfield for prefix or infix matching such as `edge_ngram`, `ngram`, `search_as_you_type`, or a controlled wildcard/prefix strategy.
4. The API contract couples too many filter concerns into a single query-string-based endpoint.

This is the direct reason subqueries that clearly belong to the visible result set are being dropped. `waterfr` is a meaningful user fragment of `Waterfront`, but under the current mapping/query design it is not a matching searchable term.

## Recommended Fix Plan

### Phase 1: Improve search matching

Add dedicated search subfields for user-friendly matching on fields like `Name` and `VenueName`.

Good options:

- `search_as_you_type` for prefix-oriented typing behavior
- `edge_ngram` for prefix matches
- `ngram` for infix matches if we explicitly want `water` to match `waterfront`

Recommended direction:

- Keep the current analyzer-backed field for relevance.
- Add a secondary subfield for partial matching.
- Query both fields, with the exact/relevance field weighted highest and the partial-match field weighted lower.

This preserves relevance while making obvious user searches work.

### Phase 2: Make tag search intentional

Decide whether tags should support:

- exact filtering only
- full-text matching
- both

If both are needed:

- keep `Tags` as `keyword` for filtering
- add a text/search subfield for tag searchability

### Phase 3: Simplify the API contract

Introduce a typed search request body, for example:

- `POST /events/search`

with a DTO that wraps:

- `query`
- `filters`
- `geo`
- `sort`
- `pagination`

Possible follow-up:

- keep `GET /events` for simple browse/list scenarios
- use `POST /events/search` for advanced search

This would remove a lot of brittle query-string parsing and make the API easier to extend.

### Phase 4: Add regression coverage

Add tests for at least:

- `waterfront` returns existing `Waterfront` results
- `waterfr` matches `Waterfront`
- `water` matches `Waterfront`
- `tor` matches `Toronto` if partial city matching is intended
- exact tag filter behavior
- Elasticsearch search vs database fallback expectations
- controller validation for geo and sort combinations

## Acceptance Criteria

- Searching `waterfront` returns expected `Waterfront` events.
- Searching `waterfr` returns expected `Waterfront` events.
- Searching `water` returns expected `Waterfront` events.
- Search behavior is consistent and intentional across name, venue, city, and tags.
- The API has a simpler typed contract for advanced search use cases.
- Search tests cover partial-match behavior and prevent regression.

## Notes

- This issue is based on a code audit, not on changes to production search settings yet.
- Reindexing alone will not fix the `waterfr`/`water` -> `Waterfront` problem unless the index mapping/query strategy changes.
