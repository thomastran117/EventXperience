using backend.Common;
using backend.DTOs;
using backend.Interfaces;
using backend.Middlewares;
using backend.Models;
using backend.Utilities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("clubs")]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> CreateClub([FromForm] ClubCreateRequest request)
        {
            UserPayload userPayload = User.GetUserPayload();

            Club club = await _clubService.CreateClub(
                name: request.Name,
                userId: userPayload.Id,
                description: request.Description,
                clubtype: request.Clubtype,
                clubimage: request.ClubImage,
                phone: request.Phone,
                email: request.Email
            );

            ClubResponse response = MapToResponse(club);

            return StatusCode(
                201,
                new ApiResponse<ClubResponse>(
                    $"The club with ID {club.Id} has been created successfully.",
                    response
                )
            );
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClub([FromForm] ClubUpdateRequest request, int id)
        {
            UserPayload userPayload = User.GetUserPayload();
            ValidateUtility.ValidatePositiveId(id);

            Club club = await _clubService.UpdateClub(
                clubId: id,
                userId: userPayload.Id,
                name: request.Name,
                description: request.Description,
                clubtype: request.Clubtype,
                clubimage: request.ClubImage,
                phone: request.Phone,
                email: request.Email
            );

            ClubResponse response = MapToResponse(club);

            return StatusCode(
                200,
                new ApiResponse<ClubResponse>(
                    $"The club with ID {id} has been updated successfully.",
                    response
                )
            );
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            UserPayload userPayload = User.GetUserPayload();
            ValidateUtility.ValidatePositiveId(id);

            await _clubService.DeleteClub(id, userPayload.Id);

            return StatusCode(
                200,
                new MessageResponse(
                    $"The club with ID {id} has been deleted successfully."
                )
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClub(int id)
        {
            ValidateUtility.ValidatePositiveId(id);

            Club club = await _clubService.GetClub(id);

            ClubResponse response = MapToResponse(club);

            return StatusCode(
                200,
                new ApiResponse<ClubResponse>(
                    $"The club with ID {id} has been fetched successfully.",
                    response
                )
            );
        }

        [HttpGet("")]
        public async Task<IActionResult> GetClubs([FromQuery] string? search)
        {
            List<Club> clubs = await _clubService.GetAllClubs(search);

            IEnumerable<ClubResponse> responses = clubs.Select(MapToResponse);

            return StatusCode(
                200,
                new ApiResponse<IEnumerable<ClubResponse>>(
                    $"The clubs have been fetched successfully.",
                    responses
                )
            );
        }

        private static ClubResponse MapToResponse(Club club)
        {
            return new ClubResponse(
                club.Id,
                club.Name,
                club.Description,
                club.Clubtype.ToString(),
                club.ClubImage
            )
            {
                Phone = club.Phone,
                Email = club.Email,
                Rating = club.Rating
            };
        }
    }
}
