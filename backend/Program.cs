using DotNetEnv;
using backend.Interfaces;
using backend.Services;
using backend.Extensions;
using backend.Configs;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMinimalSerilog();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppRedis(builder.Configuration);
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddReactCors("AllowReact");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var addresses = app.Urls.Any() ? string.Join(", ", app.Urls) : "no specific URLs";
Serilog.Log.Information("✅ Server built successfully. Listening on: {Addresses}", addresses);

app.Run();
