using backend.main.configurations.resource.database;
using backend.main.models.core;
using backend.main.models.enums;
using backend.main.services.interfaces;

using Microsoft.EntityFrameworkCore;

namespace backend.main.seeders
{
    public sealed class MockEventSeeder : ISeeder
    {
        private readonly AppDatabaseContext _dbContext;
        private readonly IEventSearchOutboxWriter _outboxWriter;
        private readonly ILogger<MockEventSeeder> _logger;

        public MockEventSeeder(
            AppDatabaseContext dbContext,
            IEventSearchOutboxWriter outboxWriter,
            ILogger<MockEventSeeder> logger
        )
        {
            _dbContext = dbContext;
            _outboxWriter = outboxWriter;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var seedEvents = BuildSeedEvents();
            var clubNames = seedEvents
                .Select(@event => @event.ClubName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var clubs = await _dbContext.Clubs
                .Where(club => clubNames.Contains(club.Name))
                .ToListAsync(cancellationToken);
            var clubLookup = clubs.ToDictionary(club => club.Name, StringComparer.OrdinalIgnoreCase);

            var missingClubs = clubNames
                .Where(name => !clubLookup.ContainsKey(name))
                .ToList();

            if (missingClubs.Count > 0)
            {
                _logger.LogWarning(
                    "[Seeders] Skipping mock events because required clubs are missing: {ClubNames}",
                    string.Join(", ", missingClubs)
                );
                return;
            }

            var targetClubIds = clubLookup.Values
                .Select(club => club.Id)
                .Distinct()
                .ToList();
            var seedEventNames = seedEvents
                .Select(@event => @event.Name)
                .ToList();

            var existingEvents = await _dbContext.Events
                .Where(@event => targetClubIds.Contains(@event.ClubId) && seedEventNames.Contains(@event.Name))
                .Select(@event => new { @event.ClubId, @event.Name })
                .ToListAsync(cancellationToken);

            var existingEventKeys = existingEvents
                .Select(@event => EventKey(@event.ClubId, @event.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingEvents = new List<Events>();

            foreach (var seedEvent in seedEvents)
            {
                var club = clubLookup[seedEvent.ClubName];
                var eventKey = EventKey(club.Id, seedEvent.Name);

                if (existingEventKeys.Contains(eventKey))
                    continue;

                missingEvents.Add(new Events
                {
                    Name = seedEvent.Name,
                    Description = seedEvent.Description,
                    Location = seedEvent.Location,
                    isPrivate = seedEvent.IsPrivate,
                    maxParticipants = seedEvent.MaxParticipants,
                    registerCost = seedEvent.RegisterCost,
                    StartTime = seedEvent.StartTimeUtc,
                    EndTime = seedEvent.EndTimeUtc,
                    ClubId = club.Id,
                    Category = seedEvent.Category,
                    VenueName = seedEvent.VenueName,
                    City = seedEvent.City,
                    Latitude = seedEvent.Latitude,
                    Longitude = seedEvent.Longitude,
                    Tags = seedEvent.Tags.ToList(),
                    CreatedAt = seedEvent.CreatedAtUtc,
                    UpdatedAt = seedEvent.UpdatedAtUtc
                });
            }

            if (missingEvents.Count == 0)
            {
                _logger.LogInformation("[Seeders] Mock events already present. No new events added.");
                return;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _dbContext.Events.AddRangeAsync(missingEvents, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var ev in missingEvents)
                _outboxWriter.StageUpsert(ev);

            await RefreshClubEventCountsAsync(targetClubIds, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "[Seeders] Seeded {Count} mock event records.",
                missingEvents.Count
            );
        }

        private async Task RefreshClubEventCountsAsync(
            IReadOnlyCollection<int> clubIds,
            CancellationToken cancellationToken
        )
        {
            if (clubIds.Count == 0)
                return;

            var now = DateTime.UtcNow;

            var eventCounts = await _dbContext.Events
                .Where(@event => clubIds.Contains(@event.ClubId))
                .GroupBy(@event => @event.ClubId)
                .Select(group => new
                {
                    ClubId = group.Key,
                    EventCount = group.Count(),
                    AvailableEventCount = group.Count(@event => @event.EndTime == null || @event.EndTime > now)
                })
                .ToListAsync(cancellationToken);

            var countLookup = eventCounts.ToDictionary(count => count.ClubId);
            var clubs = await _dbContext.Clubs
                .Where(club => clubIds.Contains(club.Id))
                .ToListAsync(cancellationToken);

            foreach (var club in clubs)
            {
                if (countLookup.TryGetValue(club.Id, out var counts))
                {
                    club.EventCount = counts.EventCount;
                    club.AvaliableEventCount = counts.AvailableEventCount;
                }
                else
                {
                    club.EventCount = 0;
                    club.AvaliableEventCount = 0;
                }

                club.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static List<SeedEventDefinition> BuildSeedEvents()
        {
            var now = DateTime.UtcNow;

            return
            [
                new SeedEventDefinition(
                    "Harbour Runners Club",
                    "Sunrise 5K Training Loop",
                    "A paced beginner-friendly run along the lake with coffee after the cooldown stretch.",
                    "Queens Quay Trailhead",
                    false,
                    60,
                    0,
                    now.AddDays(2).Date.AddHours(11),
                    now.AddDays(2).Date.AddHours(13),
                    EventCategory.Fitness,
                    "Harbourfront Trail",
                    "Toronto",
                    43.6387,
                    -79.3817,
                    ["running", "fitness", "beginner"],
                    now.AddDays(-2),
                    now.AddDays(-1)
                ),
                new SeedEventDefinition(
                    "Harbour Runners Club",
                    "Waterfront Recovery Stretch",
                    "A relaxed mobility session for runners and walkers with guided stretching and breathwork.",
                    "Harbour Square Park",
                    false,
                    35,
                    0,
                    now.AddHours(3),
                    now.AddHours(4),
                    EventCategory.Fitness,
                    "Harbour Square Park",
                    "Toronto",
                    43.6405,
                    -79.3808,
                    ["stretch", "recovery", "wellness"],
                    now.AddDays(-1),
                    now.AddHours(-6)
                ),
                new SeedEventDefinition(
                    "North Campus Builders",
                    "Founder Pitch Night",
                    "Short startup pitches, live feedback from mentors, and open networking for early builders.",
                    "Innovation Hall",
                    false,
                    120,
                    0,
                    now.AddDays(4).Date.AddHours(23),
                    now.AddDays(5).Date.AddHours(1),
                    EventCategory.Networking,
                    "Innovation Hall",
                    "Toronto",
                    43.6629,
                    -79.3957,
                    ["startup", "pitch", "networking"],
                    now.AddDays(-4),
                    now.AddDays(-2)
                ),
                new SeedEventDefinition(
                    "North Campus Builders",
                    "Product Jam Workshop",
                    "A hands-on product sprint covering idea framing, lightweight validation, and demo critiques.",
                    "Campus Design Lab",
                    false,
                    45,
                    0,
                    now.AddDays(7).Date.AddHours(17),
                    now.AddDays(7).Date.AddHours(20),
                    EventCategory.Workshop,
                    "Design Lab 204",
                    "Toronto",
                    43.6644,
                    -79.3982,
                    ["product", "workshop", "design"],
                    now.AddDays(-3),
                    now.AddDays(-1)
                ),
                new SeedEventDefinition(
                    "Lantern Social Collective",
                    "Spring Rooftop Mixer",
                    "Music, casual conversation, and light snacks for anyone looking to meet new people in the city.",
                    "King Street Rooftop",
                    false,
                    90,
                    0,
                    now.AddDays(5).Date.AddHours(23),
                    now.AddDays(6).Date.AddHours(2),
                    EventCategory.Social,
                    "Lantern House Rooftop",
                    "Toronto",
                    43.6459,
                    -79.3925,
                    ["social", "mixer", "city-life"],
                    now.AddDays(-5),
                    now.AddDays(-2)
                )
            ];
        }

        private static string EventKey(int clubId, string eventName) =>
            $"{clubId}:{eventName}".ToLowerInvariant();

        private sealed record SeedEventDefinition(
            string ClubName,
            string Name,
            string Description,
            string Location,
            bool IsPrivate,
            int MaxParticipants,
            int RegisterCost,
            DateTime StartTimeUtc,
            DateTime? EndTimeUtc,
            EventCategory Category,
            string VenueName,
            string City,
            double Latitude,
            double Longitude,
            IReadOnlyList<string> Tags,
            DateTime CreatedAtUtc,
            DateTime UpdatedAtUtc
        );
    }
}
