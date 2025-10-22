using backend.DTOs;
using backend.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace backend.Utilities
{
    public static class ErrorUtility
    {
        public static IActionResult HandleError(Exception ex)
        {
            if (ex is AppException appEx)
                return HandleAppException(appEx);

            Logger.Error(ex, "Unexpected error occurred");
            var response = new MessageResponse(
                "An unexpected error occurred.",
                success: false,
                statusCode: StatusCodes.Status500InternalServerError
            );

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        private static IActionResult HandleAppException(AppException ex)
        {
            var response = new MessageResponse(
                ex.Message,
                success: false,
                statusCode: ex.StatusCode
            );

            return new ObjectResult(response)
            {
                StatusCode = ex.StatusCode
            };
        }
    }
}
