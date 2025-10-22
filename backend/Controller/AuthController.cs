using Microsoft.AspNetCore.Mvc;

using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Utilities;

namespace backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var userToken = await _authService.LoginAsync(request.Email, request.Password)
                ?? throw new UnauthorizedException("Invalid username or password");

            var user = userToken.user;
            var token = userToken.token;

            HttpUtility.SetRefreshTokenCookie(Response, token.RefreshToken);

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

            HttpUtility.SetRefreshTokenCookie(Response, token.RefreshToken);

            return Ok(new AuthResponse(user.Id, user.Email, user.Usertype, token.AccessToken));
        }

        [HttpPost("google")]
        public async Task<IActionResult> Google()
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> Microsoft()
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        } 


        [HttpGet("verify")]
        public async Task<IActionResult> Verify()
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }               
    }
}