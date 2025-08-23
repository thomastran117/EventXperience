using Microsoft.AspNetCore.Mvc;

namespace backend.Utilities;
public static class ValidationHelper
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
}