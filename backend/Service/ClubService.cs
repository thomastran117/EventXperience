using backend.Models;
using backend.Interfaces;
using backend.Resources;

namespace backend.Services
{
    public class ClubService : IClubService
    {
        private readonly AppDatabaseContext _context;

        public ClubService(AppDatabaseContext context)
        {
            _context = context;
        }
        
        public Task<Club?> CreateClub(string name, int userId, string description, string clubtype, string? phone = null, string? email = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteClub(int clubId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Club>> GetAllClubs(string query)
        {
            throw new NotImplementedException();
        }

        public Task<Club?> GetClub(int clubId)
        {
            throw new NotImplementedException();
        }

        public Task<Club?> UpdateClub(int clubId, int userId, string name, string description, string clubtype, string? phone = null, string? email = null)
        {
            throw new NotImplementedException();
        }
    }
}