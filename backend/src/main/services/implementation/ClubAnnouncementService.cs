using backend.main.exceptions.http;
using backend.main.models.core;
using backend.main.repositories.interfaces;
using backend.main.services.interfaces;

namespace backend.main.services.implementation
{
    public class ClubAnnouncementService : IClubAnnouncementService
    {
        private readonly IClubAnnouncementRepository _announcementRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IFollowRepository _followRepository;

        public ClubAnnouncementService(
            IClubAnnouncementRepository announcementRepository,
            IClubRepository clubRepository,
            IFollowRepository followRepository)
        {
            _announcementRepository = announcementRepository;
            _clubRepository = clubRepository;
            _followRepository = followRepository;
        }

        public async Task<ClubAnnouncement> CreateAsync(int clubId, int userId, string title, string content)
        {
            var club = await _clubRepository.GetByIdAsync(clubId)
                ?? throw new ResourceNotFoundException($"Club with ID {clubId} was not found.");

            if (club.UserId != userId)
                throw new ForbiddenException("Only the club owner can post announcements.");

            var announcement = new ClubAnnouncement
            {
                ClubId = clubId,
                UserId = userId,
                Title = title,
                Content = content
            };

            return await _announcementRepository.CreateAsync(announcement);
        }

        public async Task<(List<ClubAnnouncement> Items, int TotalCount)> GetByClubIdAsync(
            int clubId, int? requestingUserId, string? search, int page, int pageSize)
        {
            var club = await _clubRepository.GetByIdAsync(clubId)
                ?? throw new ResourceNotFoundException($"Club with ID {clubId} was not found.");

            if (club.isPrivate)
            {
                if (requestingUserId == null)
                    throw new UnauthorizedException("Authentication is required to view announcements for a private club.");

                bool isOwner = club.UserId == requestingUserId.Value;
                if (!isOwner)
                {
                    var membership = await _followRepository.IsFollowingClubAsync(clubId, requestingUserId.Value);
                    if (membership == null)
                        throw new ForbiddenException("You must be a member of this club to view its announcements.");
                }
            }

            var itemsTask = _announcementRepository.GetByClubIdAsync(clubId, search, page, pageSize);
            var countTask = _announcementRepository.CountByClubIdAsync(clubId, search);
            await Task.WhenAll(itemsTask, countTask);

            return (itemsTask.Result, countTask.Result);
        }

        public async Task<ClubAnnouncement> UpdateAsync(int clubId, int announcementId, int userId, string title, string content)
        {
            var announcement = await _announcementRepository.GetByIdAsync(announcementId)
                ?? throw new ResourceNotFoundException($"Announcement with ID {announcementId} was not found.");

            if (announcement.ClubId != clubId)
                throw new ResourceNotFoundException($"Announcement with ID {announcementId} was not found.");

            if (announcement.UserId != userId)
                throw new ForbiddenException("You are not allowed to update this announcement.");

            return await _announcementRepository.UpdateAsync(announcementId, new ClubAnnouncement
            {
                Title = title,
                Content = content
            }) ?? throw new ResourceNotFoundException($"Announcement with ID {announcementId} was not found.");
        }

        public async Task DeleteAsync(int clubId, int announcementId, int userId)
        {
            var announcement = await _announcementRepository.GetByIdAsync(announcementId)
                ?? throw new ResourceNotFoundException($"Announcement with ID {announcementId} was not found.");

            if (announcement.ClubId != clubId)
                throw new ResourceNotFoundException($"Announcement with ID {announcementId} was not found.");

            if (announcement.UserId != userId)
                throw new ForbiddenException("You are not allowed to delete this announcement.");

            await _announcementRepository.DeleteAsync(announcementId);
        }

        public async Task<(List<ClubAnnouncement> Items, int TotalCount)> GetAllAdminAsync(
            string? search, int page, int pageSize)
        {
            var itemsTask = _announcementRepository.GetAllAsync(search, page, pageSize);
            var countTask = _announcementRepository.CountAllAsync(search);
            await Task.WhenAll(itemsTask, countTask);

            return (itemsTask.Result, countTask.Result);
        }
    }
}
