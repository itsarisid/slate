using System.Text.Json;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler.ExampleJobs;

/// <summary>
/// Example report-generation job.
/// </summary>
public sealed class ReportGenerationJob : IJobHandler
{
    public Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
        => Task.FromResult($"Report job '{job.Name}' completed for {DateTimeOffset.UtcNow:yyyy-MM-dd}.");
}
