using Microsoft.AspNetCore.Mvc;

using backend.Utilities;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var result = ErrorUtility.HandleError(ex) as ObjectResult;
            context.Response.StatusCode = result?.StatusCode ?? 500;
            context.Response.ContentType = "application/json";

            if (context.Response.StatusCode >= 500)
            {
                Logger.Error("There was a critical server error. Please investigate");
            }
            await context.Response.WriteAsJsonAsync(result?.Value);
        }
    }
}
