using backend.Common;
using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Middlewares;
using backend.Utilities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("follows")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [Authorize]
        [HttpPost("{id}")]
        public async Task<IActionResult> FollowClub(int id)
        {
            try
            {
                UserPayload userPayload = User.GetUserPayload();
                ValidateUtility.ValidatePositiveId(id);

                await _followService.FollowClubAsync(id, userPayload.Id);

                return StatusCode(
                    201,
                    new MessageResponse(
                        $"Successfully following the Club `{id}`."
                    )
                );
            }
            catch (Exception e)
            {
                if (e is AppException)
                    return ErrorUtility.HandleError(e);

                Logger.Error($"[FollowController] FollowClub failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> UnfollowClub(int id)
        {
            try
            {
                UserPayload userPayload = User.GetUserPayload();
                ValidateUtility.ValidatePositiveId(id);

                await _followService.UnfollowClubAsync(id, userPayload.Id);

                return StatusCode(
                    200,
                    new MessageResponse(
                        $"Successfully unfollowing the Club `{id}`."
                    )
                );
            }
            catch (Exception e)
            {
                if (e is AppException)
                    return ErrorUtility.HandleError(e);

                Logger.Error($"[FollowController] UnfollowClub failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFollows()
        {
            try
            {
                var result = await _followService.GetFollowsAsync();
                return (IActionResult)result;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    return ErrorUtility.HandleError(e);

                Logger.Error($"[FollowController] GetAllFollows failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClubFollows(int id)
        {
            try
            {
                ValidateUtility.ValidatePositiveId(id);
                var result = await _followService.GetFollowsByClubAsync(id);
                return (IActionResult)result;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    return ErrorUtility.HandleError(e);

                Logger.Error($"[FollowController] GetClubFollows failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetUserFollows()
        {
            try
            {
                UserPayload userPayload = User.GetUserPayload();
                var result = await _followService.GetFollowsByClubAsync(userPayload.Id);
                return (IActionResult)result;
            }
            catch (Exception e)
            {
                if (e is AppException)
                    return ErrorUtility.HandleError(e);

                Logger.Error($"[FollowController] GetUserFollows failed: {e}");
                return ErrorUtility.HandleError(e);
            }
        }
    }
}
