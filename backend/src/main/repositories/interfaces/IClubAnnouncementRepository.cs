using backend.main.models.core;

namespace backend.main.repositories.interfaces
{
    public interface IClubAnnouncementRepository
    {
        Task<ClubAnnouncement> CreateAsync(ClubAnnouncement announcement);
        Task<ClubAnnouncement?> GetByIdAsync(int id);
        Task<List<ClubAnnouncement>> GetByClubIdAsync(int clubId, string? search, int page, int pageSize);
        Task<int> CountByClubIdAsync(int clubId, string? search);
        Task<List<ClubAnnouncement>> GetAllAsync(string? search, int page, int pageSize);
        Task<int> CountAllAsync(string? search);
        Task<ClubAnnouncement?> UpdateAsync(int id, ClubAnnouncement updated);
        Task<bool> DeleteAsync(int id);
    }
}
