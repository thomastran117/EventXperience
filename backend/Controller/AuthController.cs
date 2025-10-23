using Microsoft.AspNetCore.Mvc;

using backend.Common;
using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
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
            UserToken userToken = await _authService.LoginAsync(request.Email, request.Password);

            User user = userToken.user;
            Token token = userToken.token;

            HttpUtility.SetRefreshTokenCookie(Response, token.RefreshToken);

            AuthResponse response = new(
                user.Id,
                user.Email,
                user.Usertype,
                token.AccessToken
            );

            return StatusCode(
                200,
                new ApiResponse<AuthResponse>(
                    $"Login successful",
                    response
                )
            );
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            _ = await _authService.SignUpAsync(request.Email, request.Password, request.Usertype);

            return StatusCode(
                200,
                new MessageResponse($"Signup successful.")
            );
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify([FromQuery] string token)
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleRequest request)
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> Microsoft([FromBody] MicrosoftRequest request)
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            string? refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Missing refresh token");

            UserToken userToken = await _authService.HandleTokensAsync(refreshToken);

            User user = userToken.user;
            Token token = userToken.token;

            HttpUtility.SetRefreshTokenCookie(Response, token.RefreshToken);

            return Ok(new AuthResponse(user.Id, user.Email, user.Usertype, token.AccessToken));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return StatusCode(
                    200,
                    new MessageResponse($"The user is already logged out.")
                );

            await _authService.HandleLogoutAsync(refreshToken);

            return StatusCode(
                200,
                new MessageResponse($"The user's logout is successful")
            );
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromQuery] string token)
        {
            throw new Exceptions.NotImplementedException("Not implemented yet");
        }
    }
}
