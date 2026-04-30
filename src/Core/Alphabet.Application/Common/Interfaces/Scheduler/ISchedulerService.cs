using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;

namespace Alphabet.Application.Common.Interfaces.Scheduler;

/// <summary>
/// Abstraction over the active scheduler engine.
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Schedules a job in the underlying engine.
    /// </summary>
    Task<string?> ScheduleJobAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a scheduled job definition.
    /// </summary>
    Task<string?> UpdateScheduleAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a job from the scheduler engine.
    /// </summary>
    Task DeleteJobAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers a job to execute immediately.
    /// </summary>
    Task<Guid> TriggerJobAsync(Job job, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken);

    /// <summary>
    /// Pauses the job in the scheduler engine.
    /// </summary>
    Task PauseJobAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Resumes the job in the scheduler engine.
    /// </summary>
    Task ResumeJobAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the effective scheduler status for a job.
    /// </summary>
    Task<string> GetJobStatusAsync(Job job, CancellationToken cancellationToken);

    /// <summary>
    /// Returns upcoming executions for dashboard views.
    /// </summary>
    Task<IReadOnlyList<UpcomingExecutionDto>> GetUpcomingExecutionsAsync(int take, CancellationToken cancellationToken);
}
