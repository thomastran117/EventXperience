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

            MessageResponse response = new MessageResponse(
                "An unexpected error occurred."
            );

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        private static IActionResult HandleAppException(AppException ex)
        {
            var response = new MessageResponse(ex.Message);

            return new ObjectResult(response)
            {
                StatusCode = ex.StatusCode
            };
        }
    }
}
