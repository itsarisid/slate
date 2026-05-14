using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Generates a scheduled productivity report.
/// </summary>
public sealed class ProductivityReportJob(ILogger<ProductivityReportJob> logger)
{
    public Task ExecuteAsync()
    {
        logger.LogInformation("Productivity report job executed.");
        return Task.CompletedTask;
    }
}
