using System.Text.Json;

using backend.main.exceptions.http;
using backend.main.models.core;
using backend.main.repositories.interfaces;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;

using Microsoft.EntityFrameworkCore;

namespace backend.main.services.implementation
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly IEventRegistrationRepository _registrationRepository;
        private readonly IEventsService _eventsService;
        private readonly ICacheService _cache;

        private static readonly TimeSpan MembershipTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ListTTL = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan LockTTL = TimeSpan.FromSeconds(10);
        private const string NullSentinel = "__null__";

        public EventRegistrationService(
            IEventRegistrationRepository registrationRepository,
            IEventsService eventsService,
            ICacheService cache)
        {
            _registrationRepository = registrationRepository;
            _eventsService = eventsService;
            _cache = cache;
        }

        private string LockKey(int eventId)
            => $"evtreg:lock:{eventId}";

        private string MembershipKey(int userId, int eventId)
            => $"evtreg:u:{userId}:e:{eventId}";

        private string EventListKey(int eventId, int page, int size)
            => $"evtreg:list:e:{eventId}:{page}:{size}";

        private string UserListKey(int userId, int page, int size)
            => $"evtreg:list:u:{userId}:{page}:{size}";

        public async Task RegisterAsync(int eventId, int userId)
        {
            var ev = await _eventsService.GetEvent(eventId);

            if (ev.registerCost > 0)
                throw new BadRequestException("Paid events require checkout");

            var lockKey = LockKey(eventId);
            var lockValue = Guid.NewGuid().ToString();
            var acquired = await _cache.AcquireLockAsync(lockKey, lockValue, LockTTL);

            if (!acquired)
                throw new ConflictException("Event registration is busy, please try again");

            try
            {
                if (await IsRegisteredAsync(eventId, userId))
                    throw new ConflictException("Already registered for this event");

                var count = await _registrationRepository.CountByEventAsync(eventId);
                if (count >= ev.maxParticipants)
                    throw new ConflictException("Event is full");

                var registration = await _registrationRepository.RegisterAsync(eventId, userId);

                await _cache.SetValueAsync(
                    MembershipKey(userId, eventId),
                    JsonSerializer.Serialize(registration),
                    MembershipTTL
                );
                await InvalidateListsAsync(userId, eventId);
            }
            catch (DbUpdateException)
            {
                // Unique constraint on (EventId, UserId) caught a duplicate that slipped past the cache check
                throw new ConflictException("Already registered for this event");
            }
            catch (Exception e)
            {
                if (e is AppException)
                    throw;

                Logger.Error($"[EventRegistrationService] RegisterAsync failed: {e}");
                throw new InternalServerErrorException();
            }
            finally
            {
                await _cache.ReleaseLockAsync(lockKey, lockValue);
            }
        }

        public async Task UnregisterAsync(int eventId, int userId)
        {
            try
            {
                await _eventsService.GetEvent(eventId);

                if (!await IsRegisteredAsync(eventId, userId))
                    throw new ResourceNotFoundException("Registration not found");

                await _registrationRepository.UnregisterAsync(eventId, userId);

                await _cache.DeleteKeyAsync(MembershipKey(userId, eventId));
                await InvalidateListsAsync(userId, eventId);
            }
            catch (Exception e)
            {
                if (e is AppException)
                    throw;

                Logger.Error($"[EventRegistrationService] UnregisterAsync failed: {e}");
                throw new InternalServerErrorException();
            }
        }

        public async Task<bool> IsRegisteredAsync(int eventId, int userId)
        {
            try
            {
                var key = MembershipKey(userId, eventId);
                var cached = await _cache.GetValueAsync(key);

                if (cached != null)
                    return cached != NullSentinel;

                var registration = await _registrationRepository.IsRegisteredAsync(eventId, userId);

                if (registration == null)
                {
                    await _cache.SetValueAsync(key, NullSentinel, MembershipTTL);
                    return false;
                }

                await _cache.SetValueAsync(key, JsonSerializer.Serialize(registration), MembershipTTL);
                return true;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    throw;

                Logger.Error($"[EventRegistrationService] IsRegisteredAsync failed: {e}");
                throw new InternalServerErrorException();
            }
        }

        public async Task<IEnumerable<EventRegistration>> GetRegistrationsByEventAsync(int eventId, int page = 1, int pageSize = 20)
        {
            try
            {
                var key = EventListKey(eventId, page, pageSize);
                var cached = await _cache.GetValueAsync(key);

                if (cached != null)
                    return JsonSerializer.Deserialize<List<EventRegistration>>(cached)!;

                var registrations = (await _registrationRepository.GetRegistrationsByEventAsync(eventId, page, pageSize)).ToList();

                await _cache.SetValueAsync(key, JsonSerializer.Serialize(registrations), ListTTL);
                return registrations;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    throw;

                Logger.Error($"[EventRegistrationService] GetRegistrationsByEventAsync failed: {e}");
                throw new InternalServerErrorException();
            }
        }

        public async Task<IEnumerable<EventRegistration>> GetRegistrationsByUserAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var key = UserListKey(userId, page, pageSize);
                var cached = await _cache.GetValueAsync(key);

                if (cached != null)
                    return JsonSerializer.Deserialize<List<EventRegistration>>(cached)!;

                var registrations = (await _registrationRepository.GetRegistrationsByUserAsync(userId, page, pageSize)).ToList();

                await _cache.SetValueAsync(key, JsonSerializer.Serialize(registrations), ListTTL);
                return registrations;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    throw;

                Logger.Error($"[EventRegistrationService] GetRegistrationsByUserAsync failed: {e}");
                throw new InternalServerErrorException();
            }
        }

        private async Task InvalidateListsAsync(int userId, int eventId)
        {
            var server = _cache.GetServer();
            var eventListKeys = _cache.ScanKeys(server, $"evtreg:list:e:{eventId}:*");
            var userListKeys = _cache.ScanKeys(server, $"evtreg:list:u:{userId}:*");

            foreach (var key in eventListKeys.Concat(userListKeys))
                await _cache.DeleteKeyAsync(key);
        }
    }
}
