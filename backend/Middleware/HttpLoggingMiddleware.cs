namespace backend.Middlewares;
using System.Diagnostics;

public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        var responseTime = stopwatch.ElapsedMilliseconds;

        var statusCode = context.Response.StatusCode;
        var method = context.Request.Method;
        var path = context.Request.Path;

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = statusCode switch
        {
            >= 200 and < 300 => ConsoleColor.Green,
            >= 300 and < 400 => ConsoleColor.Cyan,
            >= 400 and < 500 => ConsoleColor.Yellow,
            >= 500           => ConsoleColor.Red,
            _                 => ConsoleColor.Gray
        };

        var logMessage =
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] " +
            $"{method} {path} → {statusCode} ({responseTime} ms)";

        _logger.LogInformation("{Method} {Path} → {StatusCode} ({Elapsed} ms)",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            responseTime);


        Console.ForegroundColor = originalColor;
    }
}
