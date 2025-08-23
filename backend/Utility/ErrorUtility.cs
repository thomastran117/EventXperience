using backend.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace backend.Utilities;
public static class ErrorUtility
{
    public static IActionResult HandleError(Exception ex, ILogger logger)
    {
        switch (ex)
        {
            case ConflictException conflictException:
                return HandleConflictException(conflictException);

            case NotFoundException notFoundException:
                return HandleNotFoundException(notFoundException);

            case UnauthorizedException unauthorizedException:
                return HandleUnauthorizedException(unauthorizedException);

            case ForbiddenException forbiddenException:
                return HandleForbiddenException(forbiddenException);
            default:
                return HandleGenericException(ex, logger);
        }
    }

    private static IActionResult HandleConflictException(ConflictException ex)
    {
        return new ConflictObjectResult(new
        {
            message = ex.Message,
            errorCode = "EMAIL_CONFLICT"
        });
    }

    private static IActionResult HandleNotFoundException(NotFoundException ex)
    {
        return new NotFoundObjectResult(new
        {
            message = ex.Message,
            errorCode = "USER_NOT_FOUND"
        });
    }

    private static IActionResult HandleUnauthorizedException(UnauthorizedException ex)
    {
        return new UnauthorizedObjectResult(new
        {
            message = ex.Message,
            errorCode = "INVALID_CREDENTIALS"
        });
    }

    private static IActionResult HandleForbiddenException(ForbiddenException ex)
    {
        var response = new
        {
            message = ex.Message,
            errorCode = "INVALID_CREDENTIALS"
        };
        
        return new ObjectResult(response)
        {
            StatusCode = 403
        };
    }

    private static IActionResult HandleGenericException(Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Unexpected error occurred");
        return new ObjectResult(new
        {
            message = "An unexpected error occurred.",
            errorCode = "SERVER_ERROR"
        })
        {
            StatusCode = 500
        };
    }
}
