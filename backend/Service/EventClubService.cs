using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Resources;


namespace backend.Services
{
    public class EventClubService : IEventClubService
    {
        private readonly AppDatabaseContext _context;

        public EventClubService(AppDatabaseContext context)
        {
            _context = context;
        }

        public Task<EventClub?> CreateEvent(string name, string description, string location, string eventImage, DateTime startTime, int clubId, string? intensity = null, DateTime? endTime = null)
        {
            throw new NotImplementedException("Not implemented");
        }

        public Task<bool> DeleteEvent(int eventId)
        {
            throw new NotImplementedException("Not implemented");
        }

        public Task<List<EventClub>> GetAllEvents(string? query = null)
        {
            throw new NotImplementedException("Not implemented");
        }

        public Task<EventClub?> GetEvent(int eventId)
        {
            throw new NotImplementedException("Not implemented");
        }

        public Task<EventClub?> UpdateEvent(int eventId, string name, string description, string location, string eventImage, DateTime startTime, int clubId, string? intensity = null, DateTime? endTime = null)
        {
            throw new NotImplementedException("Not implemented");
        }
    }
}
