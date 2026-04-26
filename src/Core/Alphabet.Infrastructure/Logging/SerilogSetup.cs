using Microsoft.Extensions.Configuration;
using Serilog;

namespace Alphabet.Infrastructure.Logging;

/// <summary>
/// Configures Serilog for the application.
/// </summary>
public static class SerilogSetup
{
    public static void Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/alphabet-.log", rollingInterval: RollingInterval.Day);
    }
}
