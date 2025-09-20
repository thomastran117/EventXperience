using Serilog;
using Serilog.Events;

namespace backend.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder UseMinimalSerilog(this IHostBuilder host)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}")
            .CreateLogger();

        return host.UseSerilog();
    }
}