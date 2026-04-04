using backend.main.models.core;

namespace backend.main.repositories.interfaces
{
    public interface IEventRegistrationRepository
    {
        Task<EventRegistration> RegisterAsync(int eventId, int userId);
        Task<bool> UnregisterAsync(int eventId, int userId);
        Task<EventRegistration?> IsRegisteredAsync(int eventId, int userId);
        Task<IEnumerable<EventRegistration>> GetRegistrationsByEventAsync(int eventId, int page = 1, int pageSize = 20);
        Task<IEnumerable<EventRegistration>> GetRegistrationsByUserAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> CountByEventAsync(int eventId);
    }
}
