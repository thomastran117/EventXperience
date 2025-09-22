using DotNetEnv;
using backend.Interfaces;
using backend.Services;
using backend.Extensions;
using backend.Configs;
using backend.Utilities;
using Serilog;

Env.Load();

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var addresses = app.Urls.Any() ? string.Join(", ", app.Urls) : "no specific URLs";
Logger.Info("Server built successfully. Listening on: ...");

app.Run();
