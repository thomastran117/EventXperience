using backend.Interfaces;
using backend.DTOs;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
            var userId = User.GetUserId();
            var club = await _clubService.CreateClub(request.Name, userId, request.Description, request.Clubtype, request.ClubImage, request.Phone, request.Email);
            var response = new ClubResponse(
                club.Id,
                club.Name,
                club.Description,
                club.Clubtype,
                club.ClubImage
            )
            {
                Phone = club.Phone,
                Email = club.Email,
                Rating = club.Rating
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClub([FromForm] ClubUpdateRequest request, int id)
        {
            var userId = User.GetUserId();
            HttpUtility.ValidatePositiveId(id);
            var club = await _clubService.UpdateClub(id, userId, request.Name, request.Description, request.Clubtype, request.ClubImage, request.Phone, request.Email);
            var response = new ClubResponse(
                club.Id,
                club.Name,
                club.Description,
                club.Clubtype,
                club.ClubImage
            )
            {
                Phone = club.Phone,
                Email = club.Email,
                Rating = club.Rating
            };

            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var userId = User.GetUserId();
            HttpUtility.ValidatePositiveId(id);
            var result = await _clubService.DeleteClub(userId, id);

            if (result)
            {
                return Ok(new MessageResponse("Club deleted successfully.", true, StatusCodes.Status200OK));
            }
            else
            {
                var response = new MessageResponse(
                    "An unexpected error occurred.",
                    success: false,
                    statusCode: StatusCodes.Status500InternalServerError
                );

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClub(int id)
        {
            HttpUtility.ValidatePositiveId(id);
            var club = await _clubService.GetClub(id);

            var response = new ClubResponse(
                club.Id,
                club.Name,
                club.Description,
                club.Clubtype,
                club.ClubImage
            )
            {
                Phone = club.Phone,
                Email = club.Email,
                Rating = club.Rating
            };

            return Ok(response);
        }


        [HttpGet("")]
        public async Task<IActionResult> GetClubs([FromQuery] string? search)
        {
            var clubs = await _clubService.GetAllClubs(search);

            var responses = clubs.Select(club => new ClubResponse(
                club.Id,
                club.Name,
                club.Description,
                club.Clubtype,
                club.ClubImage
            )
            {
                Phone = club.Phone,
                Email = club.Email,
                Rating = club.Rating
            });

            return Ok(responses);
        }
    }
}