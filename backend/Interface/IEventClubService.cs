using backend.Models;

namespace backend.Interfaces;

public interface IEventClubService
{
    Task<List<EventClub>> GetAllEvents(string? query = null);
    Task<EventClub?> GetEvent(int eventId);
    Task<EventClub?> CreateEvent(
        string name,
        string description,
        string location,
        string eventImage,
        DateTime startTime,
        int clubId,
        string? intensity = null,
        DateTime? endTime = null
    );
    Task<EventClub?> UpdateEvent(
        int eventId,
        string name,
        string description,
        string location,
        string eventImage,
        DateTime startTime,
        int clubId,
        string? intensity = null,
        DateTime? endTime = null
    );
    Task<bool> DeleteEvent(int eventId);
}
