using backend.Interfaces;
using backend.DTOs;
using backend.Exceptions;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class UserController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    public UserController(ITokenService tokenService, IAuthService authService, ILogger<UserController> logger)
    {
        _tokenService = tokenService;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.LoginAsync(request.Username, request.Password);

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
            var user = await _authService.SignUpAsync(request.Username, request.Password, request.Usertype);
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
