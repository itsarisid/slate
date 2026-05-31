using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Placeholder implementation for future Quartz support.
/// </summary>
public sealed class QuartzSchedulerService : ISchedulerService
{
    /// <summary>
    /// Schedule job async.
    /// </summary>
    public Task<string?> ScheduleJobAsync(Job job, CancellationToken cancellationToken)
    => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Update schedule async.
    /// </summary>

    public Task<string?> UpdateScheduleAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Delete job async.
    /// </summary>

    public Task DeleteJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Trigger job async.
    /// </summary>

    public Task<Guid> TriggerJobAsync(Job job, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Pause job async.
    /// </summary>

    public Task PauseJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Resume job async.
    /// </summary>

    public Task ResumeJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");
    /// <summary>
    /// Get job status async.
    /// </summary>

    public Task<string> GetJobStatusAsync(Job job, CancellationToken cancellationToken)
        => Task.FromResult("QuartzNotConfigured");
    /// <summary>
    /// Get upcoming executions async.
    /// </summary>

    public Task<IReadOnlyList<UpcomingExecutionDto>> GetUpcomingExecutionsAsync(int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<UpcomingExecutionDto>>([]);
}
