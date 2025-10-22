using backend.Interfaces;
using backend.DTOs;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Exceptions;

namespace backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;

        public AuthController(ITokenService tokenService, IAuthService authService)
        {
            _tokenService = tokenService;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.LoginAsync(request.Email, request.Password)
                ?? throw new UnauthorizedException("Invalid username or password");
                
            var accessToken = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            SetRefreshTokenCookie(refreshToken);

            return Ok(new AuthResponse(user.Id, user.Email, user.Usertype, accessToken));
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var user = await _authService.SignUpAsync(request.Email, request.Password, request.Usertype)
                ?? throw new InternalServerException("An internal server error occured when signing up");

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Registration successful. Please login."
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Missing refresh token");

            var principal = _tokenService.ValidateRefreshToken(refreshToken);
            if (principal == null)
                throw new UnauthorizedException("Invalid or expired refresh token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedException("Invalid or expired refresh token");

            var user = _authService
                .GetUserByIdAsync(int.Parse(userId)).Result
                ?? throw new NotFoundException($"User with the id {userId} can not be found");

            var rotated = _tokenService.RotateRefreshToken(user, refreshToken);
            if (rotated == null)
                throw new UnauthorizedException("Invalid or expired refresh token");

            SetRefreshTokenCookie(rotated.Value.refreshToken);

            return Ok(new { accessToken = rotated.Value.accessToken });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}