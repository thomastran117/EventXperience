using backend.Interfaces;
using backend.DTOs;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Exceptions;

namespace backend.Controllers;

[ApiController]
[Route("clubs")]
public class ClubController : ControllerBase
{
    private readonly IClubService _clubService;
    private readonly ILogger<UserController> _logger;

    public ClubController(IClubService clubService)
    {
        _clubService = clubService;
    }

    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> CreateClub([FromForm] ClubCreateRequest request)
    {
        try
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
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClub([FromForm] ClubUpdateRequest request, int id)
    {
        try
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
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClub(int id)
    {
        try
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
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClub(int id)
    {
        try
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
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }


    [HttpGet("")]
    public async Task<IActionResult> GetClubs([FromQuery] string? search)
    {
        try
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
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }
}
