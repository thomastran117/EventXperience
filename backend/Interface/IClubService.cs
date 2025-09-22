using backend.Models;

namespace backend.Interfaces;

public interface IClubService
{
    Task<List<Club>> GetAllClubs(string query);
    Task<Club?> GetClub(int clubId);
    Task<Club?> CreateClub(string name, int userId, string description, string clubtype, string? phone = null, string? email = null);
    Task<Club?> UpdateClub(int clubId, int userId, string name, string description, string clubtype, string? phone = null, string? email = null);
    Task<bool> DeleteClub(int clubId, int userId);
}
