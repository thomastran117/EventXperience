using backend.Interfaces;
using backend.DTOs;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.LoginAsync(request.Email, request.Password);

            var token = _tokenService.GenerateJwtToken(user!);

            return Ok(new AuthResponse(user!.Id, user!.Email, user!.Usertype, token));
        }
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var user = await _authService.SignUpAsync(request.Email, request.Password, request.Usertype);
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Registration successful. Please login."
            });
        }
        catch (Exception ex)
        {
            return ErrorUtility.HandleError(ex, _logger);
        }
    }
}
