using System.Threading.RateLimiting;

namespace backend.Middlewares
{
    public static class RateLimiterMiddleware
    {
        public static IServiceCollection AddAppRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();
                    }

                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests. Slow down.",
                        path = context.HttpContext.Request.Path,
                        retryAfter = retryAfter.ToString()
                    }, token);
                };

                options.AddPolicy("Fixed", httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy("UserPolicy", httpContext =>
                {
                    var userId = httpContext.User.Identity?.IsAuthenticated == true
                        ? httpContext.User.FindFirst("sub")?.Value ?? "unknown"
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";

                    return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 20,
                            TokensPerPeriod = 20,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            AutoReplenishment = true
                        });
                });

            });

            return services;
        }
    }
}
