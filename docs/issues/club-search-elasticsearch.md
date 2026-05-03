# Club Search Elasticsearch Migration

## Summary

Club search currently uses MySQL substring matching and appears to support fragment queries well. If we transition clubs onto Elasticsearch, we should do it intentionally so we gain consistency with the broader search stack without regressing the current search experience.

The key requirement is that club search must preserve fragment-style matching such as:

- `water` -> `Waterfront Running Club`
- `waterfr` -> `Waterfront Running Club`

This is especially important because event search currently demonstrates the opposite failure mode when Elasticsearch is configured only for analyzed term matching.

## Current State

Club search is exposed through:

- `GET /clubs?search=...`

Code references:

- `backend/src/main/controllers/implementation/ClubController.cs:146`
- `backend/src/main/services/implementation/ClubService.cs:174`
- `backend/src/main/repositories/implementation/ClubRepository.cs:97`

The current repository implementation uses SQL `LIKE '%term%'` over:

- `Name`
- `Description`

Code reference:

- `backend/src/main/repositories/implementation/ClubRepository.cs:109`

That means the current system is simple, but it has one important user-facing property: partial substrings are likely to match.

## Why Move Clubs to Elasticsearch

Moving clubs to Elasticsearch can still make sense for a few reasons:

- align search architecture across events, clubs, and club posts
- support better relevance ranking than raw `LIKE`
- make it easier to add filters, popularity boosts, and richer search behavior later
- reduce divergence between search implementations across entities

## Main Risk

If we migrate clubs to Elasticsearch using the same style as the current event search implementation, we risk introducing the same regression:

- exact token matches work
- fragment or prefix queries fail unexpectedly

That would be a clear downgrade from the current club search behavior.

## Migration Requirements

### 1. Preserve current fragment matching

Club search in Elasticsearch should support at minimum:

- exact matches
- prefix matches
- fragment-style partial matches for user-visible names

Examples:

- `waterfront` should match `Waterfront Running Club`
- `waterfr` should match `Waterfront Running Club`
- `water` should match `Waterfront Running Club`

### 2. Keep relevance quality better than SQL `LIKE`

We should not migrate just to recreate MySQL behavior with more infrastructure. The Elasticsearch version should improve on ranking while preserving discoverability.

Recommended direction:

- keep a primary analyzed field for normal relevance
- add a partial-match search subfield for fragment discovery
- query both with different weights

### 3. Define the club search document explicitly

A club search document likely needs at least:

- `Id`
- `Name`
- `Description`
- `Clubtype`
- `Location`
- `MemberCount`
- `EventCount`
- `AvaliableEventCount`
- `Rating`
- `IsPrivate`
- `CreatedAt`
- `UpdatedAt`

Suggested searchable fields:

- `Name`
- `Description`
- optionally `Location`

Suggested ranking inputs:

- `MemberCount`
- `EventCount`
- `AvaliableEventCount`
- `Rating`

### 4. Plan the ingestion path before implementation

Events already have an outbox/indexing pipeline. Clubs do not appear to have equivalent Elasticsearch indexing infrastructure yet.

So this migration likely needs:

- a `ClubDocument`
- a mapper from `Club` to `ClubDocument`
- a club search service interface and implementation
- index bootstrap and mappings
- a reindex path
- an update/index synchronization strategy for create/update/delete

### 5. Preserve fallback behavior

If Elasticsearch is unavailable, club search should fall back safely rather than fail completely.

A reasonable pattern would mirror event and club-post search:

- Elasticsearch as primary
- MySQL `LIKE` as fallback

## Recommended Implementation Direction

### Phase 1: Add Elasticsearch support for clubs

Create:

- `ClubDocument`
- `IClubSearchService`
- `ClubSearchService`
- index mapping and bootstrap

### Phase 2: Add ingestion/reindex support

Create:

- mapper from `Club` to document
- sync path for create/update/delete
- full reindex support

### Phase 3: Switch club search reads to Elasticsearch-first

Update `ClubService.GetAllClubs(...)` to:

- query Elasticsearch first
- fall back to repository `LIKE` search if Elasticsearch is disabled or unavailable

### Phase 4: Add regression coverage

Add tests for:

- `waterfront` matches `Waterfront Running Club`
- `waterfr` matches `Waterfront Running Club`
- `water` matches `Waterfront Running Club`
- ranking remains stable and intentional
- fallback to MySQL works when Elasticsearch is unavailable

## Acceptance Criteria

- Club search uses Elasticsearch as the primary search engine.
- Club search preserves current partial-match discoverability.
- `waterfront`, `waterfr`, and `water` all return expected club results.
- Club search has a safe database fallback.
- Tests cover the fragment-matching behavior so clubs do not regress into the current event-search failure mode.

## Notes

- This should be treated as a migration with non-regression requirements, not just an infrastructure swap.
- The event search audit is a useful cautionary reference here: Elasticsearch must be configured for product expectations, not just default analyzed term matching.
