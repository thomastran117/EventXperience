using backend.Common;
using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Utilities;

using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> LocalAuthenticate([FromBody] LoginRequest request)
        {
            try
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
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] LocalAuthenticate failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> LocalSignup([FromBody] SignUpRequest request)
        {
            try
            {
                await _authService.SignUpAsync(request.Email, request.Password, request.Usertype);

                return StatusCode(
                    200,
                    new MessageResponse("Verification email sent.")
                );
            }
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] LocalSignup failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> LocalVerify([FromQuery] string token)
        {
            try
            {
                UserToken userToken = await _authService.VerifyAsync(token);
                User user = userToken.user;
                Token authToken = userToken.token;

                HttpUtility.SetRefreshTokenCookie(Response, authToken.RefreshToken);

                AuthResponse response = new(
                    user.Id,
                    user.Email,
                    user.Usertype,
                    authToken.AccessToken
                );

                return StatusCode(
                    200,
                    new ApiResponse<AuthResponse>(
                        $"Verification successful",
                        response
                    )
                );
            }
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] LocalVerify failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuthenticate([FromBody] GoogleRequest request)
        {
            try
            {
                UserToken userToken = await _authService.GoogleAsync(request.Token);

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
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] GoogleAuthenticate failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftAuthenticate([FromBody] MicrosoftRequest request)
        {
            try
            {
                UserToken userToken = await _authService.MicrosoftAsync(request.Token);

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
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] ChangePassword failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
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
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] Refresh failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
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
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] Logout failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request.Email);

                return StatusCode(
                    200,
                    new MessageResponse("If the account exist, we send a reset email")
                );
            }
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] ForgotPassword failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromQuery] string token)
        {
            try
            {
                await _authService.ChangePasswordAsync(token, request.Password);

                return StatusCode(
                    200,
                    new MessageResponse("Password reset successful. Please login")
                );
            }
            catch (Exception e)
            {
                if (e is AppException) return ErrorUtility.HandleError(e);

                Logger.Error($"[AuthController] ChangePassword failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }
    }
}
