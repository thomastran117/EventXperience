using backend.DTOs;
using backend.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Utilities;

public static class ErrorUtility
{
    public static IActionResult HandleError(Exception ex, ILogger logger)
    {
        return ex switch
        {
            ConflictException conflictException => HandleConflictException(conflictException),
            NotFoundException notFoundException => HandleNotFoundException(notFoundException),
            UnauthorizedException unauthorizedException => HandleUnauthorizedException(unauthorizedException),
            ForbiddenException forbiddenException => HandleForbiddenException(forbiddenException),
            _ => HandleGenericException(ex, logger)
        };
    }

    private static IActionResult HandleConflictException(ConflictException ex)
    {
        var response = new MessageResponse(
            ex.Message,
            success: false,
            statusCode: StatusCodes.Status409Conflict
        );
        return new ConflictObjectResult(response);
    }

    private static IActionResult HandleNotFoundException(NotFoundException ex)
    {
        var response = new MessageResponse(
            ex.Message,
            success: false,
            statusCode: StatusCodes.Status404NotFound
        );
        return new NotFoundObjectResult(response);
    }

    private static IActionResult HandleUnauthorizedException(UnauthorizedException ex)
    {
        var response = new MessageResponse(
            ex.Message,
            success: false,
            statusCode: StatusCodes.Status401Unauthorized
        );
        return new UnauthorizedObjectResult(response);
    }

    private static IActionResult HandleForbiddenException(ForbiddenException ex)
    {
        var response = new MessageResponse(
            ex.Message,
            success: false,
            statusCode: StatusCodes.Status403Forbidden
        );
        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private static IActionResult HandleGenericException(Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Unexpected error occurred");

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
}
