using System.Text.Json;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler.ExampleJobs;

/// <summary>
/// Example cleanup job implementation.
/// </summary>
public sealed class CleanupJob : IJobHandler
{
    /// <summary>
    /// Execute async.
    /// </summary>
    public Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
    => Task.FromResult($"Cleanup job '{job.Name}' ran successfully.");
}
