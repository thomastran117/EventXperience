using Microsoft.AspNetCore.Mvc;

using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;

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
            var userToken = await _authService.LoginAsync(request.Email, request.Password)
                ?? throw new UnauthorizedException("Invalid username or password");

            var user = userToken.user;
            var token = userToken.token;

            SetRefreshTokenCookie(token.RefreshToken);

            return Ok(new AuthResponse(user.Id, user.Email, user.Usertype, token.AccessToken));
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
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Missing refresh token");

            var userToken = await _authService.HandleTokensAsync(refreshToken)
                ?? throw new UnauthorizedException("Invalid or expired refresh token");

            var user = userToken.user;
            var token = userToken.token;

            SetRefreshTokenCookie(token.RefreshToken);

            return Ok(new AuthResponse(user.Id, user.Email, user.Usertype, token.AccessToken));
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