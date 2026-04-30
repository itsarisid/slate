using System.Text.Json;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler.ExampleJobs;

/// <summary>
/// Example code-execution scheduler job.
/// </summary>
public sealed class SampleJob : IJobHandler
{
    public Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
        => Task.FromResult($"Sample job '{job.Name}' executed at {DateTimeOffset.UtcNow:O}.");
}
