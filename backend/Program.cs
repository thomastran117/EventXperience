using Serilog;

using Microsoft.Extensions.FileProviders;

using backend.Config;
using backend.Interfaces;
using backend.Middlewares;
using backend.Services;
using backend.Utilities;

Logger.Configure(o =>
{
    o.EnableFileLogging = true;
    o.MinFileLevel = backend.Utilities.LogLevel.Warn;
    o.LogDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "logs"));
});

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMinimalSerilog();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers(options =>
{
    options.Conventions.Insert(0, new RoutePrefixConvention("api"));
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppRedis(builder.Configuration);
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddReactCors("AllowReact");

var app = builder.Build();

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    opts.EnrichDiagnosticContext = (ctx, http) =>
    {
        ctx.Set("RequestHost", http.Request.Host.Value);
        ctx.Set("RequestScheme", http.Request.Scheme);
        ctx.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
    };
});

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "www")),
    RequestPath = "/public"
});

app.MapControllers();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == StatusCodes.Status404NotFound &&
        !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Resource not found",
            code = 404,
            path = context.Request.Path
        });
    }
});

app.MapGet("/api", () =>
{
    return Results.Json(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
});

app.MapGet("/health", () =>
{
    return Results.Json(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow
    });
});

var addresses = app.Urls.Any() ? string.Join(", ", app.Urls) : "no specific URLs";
Logger.Info("Server built successfully. Listening on: ...");

app.Run();
