using backend.main.models.core;

namespace backend.main.services.interfaces
{
    public interface IClubAnnouncementService
    {
        Task<ClubAnnouncement> CreateAsync(int clubId, int userId, string title, string content);
        Task<(List<ClubAnnouncement> Items, int TotalCount)> GetByClubIdAsync(
            int clubId, int? requestingUserId, string? search, int page, int pageSize);
        Task<ClubAnnouncement> UpdateAsync(int clubId, int announcementId, int userId, string title, string content);
        Task DeleteAsync(int clubId, int announcementId, int userId);
        Task<(List<ClubAnnouncement> Items, int TotalCount)> GetAllAdminAsync(
            string? search, int page, int pageSize);
    }
}
