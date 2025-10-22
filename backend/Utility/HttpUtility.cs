using backend.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Utilities
{
    public static class HttpUtility
    {
        public static IActionResult? ValidatePositiveId(int id)
        {
            if (id <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = "ID must be a positive integer",
                    errorCode = "INVALID_ID"
                });
            }

            return null;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            Logger.Debug("here");
            if (userIdClaim == null)
            {
                throw new UnauthorizedException();
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}