using backend.main.configurations.security;
using backend.main.dtos.requests.clubannouncement;
using backend.main.dtos.responses.clubannouncement;
using backend.main.dtos.responses.general;
using backend.main.models.core;
using backend.main.services.interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.main.implementation.controllers
{
    [ApiController]
    [Route("clubs")]
    public class ClubAnnouncementController : ControllerBase
    {
        private readonly IClubAnnouncementService _announcementService;

        public ClubAnnouncementController(IClubAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [Authorize]
        [HttpPost("{clubId}/announcements")]
        public async Task<IActionResult> CreateAnnouncement(int clubId, [FromBody] ClubAnnouncementCreateRequest request)
        {
            var userPayload = User.GetUserPayload();

            ClubAnnouncement announcement = await _announcementService.CreateAsync(
                clubId, userPayload.Id, request.Title, request.Content);

            return StatusCode(
                201,
                new ApiResponse<ClubAnnouncementResponse>(
                    $"Announcement for club with ID {clubId} has been created successfully.",
                    MapToResponse(announcement)
                )
            );
        }

        [AllowAnonymous]
        [HttpGet("{clubId}/announcements")]
        public async Task<IActionResult> GetAnnouncements(
            int clubId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = User.GetUserPayload().Id;

            var (items, totalCount) = await _announcementService.GetByClubIdAsync(
                clubId, userId, search, page, pageSize);

            var paged = new PagedResponse<ClubAnnouncementResponse>(
                items.Select(MapToResponse),
                totalCount,
                page,
                pageSize
            );

            return StatusCode(
                200,
                new ApiResponse<PagedResponse<ClubAnnouncementResponse>>(
                    $"Announcements for club with ID {clubId} have been fetched successfully.",
                    paged
                )
            );
        }

        [Authorize]
        [HttpPut("{clubId}/announcements/{id}")]
        public async Task<IActionResult> UpdateAnnouncement(
            int clubId,
            int id,
            [FromBody] ClubAnnouncementUpdateRequest request)
        {
            var userPayload = User.GetUserPayload();

            ClubAnnouncement announcement = await _announcementService.UpdateAsync(
                clubId, id, userPayload.Id, request.Title, request.Content);

            return StatusCode(
                200,
                new ApiResponse<ClubAnnouncementResponse>(
                    $"Announcement with ID {id} has been updated successfully.",
                    MapToResponse(announcement)
                )
            );
        }

        [Authorize]
        [HttpDelete("{clubId}/announcements/{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int clubId, int id)
        {
            var userPayload = User.GetUserPayload();

            await _announcementService.DeleteAsync(clubId, id, userPayload.Id);

            return StatusCode(
                200,
                new MessageResponse(
                    $"Announcement with ID {id} has been deleted successfully."
                )
            );
        }

        private static ClubAnnouncementResponse MapToResponse(ClubAnnouncement a) =>
            new(a.Id, a.ClubId, a.UserId, a.Title, a.Content, a.CreatedAt, a.UpdatedAt);
    }

    [ApiController]
    [Route("admin/clubs")]
    [Authorize("AdminOnly")]
    public class AdminClubAnnouncementController : ControllerBase
    {
        private readonly IClubAnnouncementService _announcementService;

        public AdminClubAnnouncementController(IClubAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet("announcements")]
        public async Task<IActionResult> GetAllAnnouncements(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, totalCount) = await _announcementService.GetAllAdminAsync(search, page, pageSize);

            var paged = new PagedResponse<ClubAnnouncementResponse>(
                items.Select(MapToResponse),
                totalCount,
                page,
                pageSize
            );

            return StatusCode(
                200,
                new ApiResponse<PagedResponse<ClubAnnouncementResponse>>(
                    "All announcements have been fetched successfully.",
                    paged
                )
            );
        }

        private static ClubAnnouncementResponse MapToResponse(ClubAnnouncement a) =>
            new(a.Id, a.ClubId, a.UserId, a.Title, a.Content, a.CreatedAt, a.UpdatedAt);
    }
}
