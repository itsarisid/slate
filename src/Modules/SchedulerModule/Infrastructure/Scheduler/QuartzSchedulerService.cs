using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Placeholder implementation for future Quartz support.
/// </summary>
public sealed class QuartzSchedulerService : ISchedulerService
{
    public Task<string?> ScheduleJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task<string?> UpdateScheduleAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task DeleteJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task<Guid> TriggerJobAsync(Job job, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task PauseJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task ResumeJobAsync(Job job, CancellationToken cancellationToken)
        => throw new NotSupportedException("Quartz support is not yet enabled. Set Scheduler:Provider to Hangfire.");

    public Task<string> GetJobStatusAsync(Job job, CancellationToken cancellationToken)
        => Task.FromResult("QuartzNotConfigured");

    public Task<IReadOnlyList<UpcomingExecutionDto>> GetUpcomingExecutionsAsync(int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<UpcomingExecutionDto>>([]);
}
