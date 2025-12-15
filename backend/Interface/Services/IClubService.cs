using backend.Models;

namespace backend.Interfaces
{
    public interface IClubService
    {
        Task<List<Club>> GetAllClubs(
            string? search = null,
            int page = 1,
            int pageSize = 20
        );
        Task<Club> GetClub(int clubId);
        Task<Club> CreateClub(string name, int userId, string description, string clubtype, IFormFile clubimage, string? phone = null, string? email = null);
        Task<Club> UpdateClub(int clubId, int userId, string name, string description, string clubtype, IFormFile clubimage, string? phone = null, string? email = null);
        Task DeleteClub(int clubId, int userId);
    }
}
