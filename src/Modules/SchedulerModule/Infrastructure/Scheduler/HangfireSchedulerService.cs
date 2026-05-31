using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Hangfire;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Hangfire-backed scheduler service.
/// </summary>
public sealed class HangfireSchedulerService(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager,
    IJobExecutionService jobExecutionService,
    IJobRepository jobRepository) : ISchedulerService
{
    /// <summary>
    /// Schedule job async.
    /// </summary>
    public Task<string?> ScheduleJobAsync(Job job, CancellationToken cancellationToken)
    {
        var schedulerJobId = BuildRecurringJobId(job);
        switch (job.ScheduleType)
        {
            case ScheduleType.Cron:
                recurringJobManager.AddOrUpdate<JobExecutor>(
                    schedulerJobId,
                    executor => executor.ExecuteScheduledAsync(job.Id),
                    job.ScheduleExpression!,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(job.Timezone)
                    });
                return Task.FromResult<string?>(schedulerJobId);
            case ScheduleType.Interval:
                var cron = BuildIntervalCron(job.IntervalSeconds ?? 300);
                recurringJobManager.AddOrUpdate<JobExecutor>(
                    schedulerJobId,
                    executor => executor.ExecuteScheduledAsync(job.Id),
                    cron,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(job.Timezone)
                    });
                return Task.FromResult<string?>(schedulerJobId);
            case ScheduleType.OneTime:
                if (job.RunAt.HasValue)
                {
                    backgroundJobClient.Schedule<JobExecutor>(
                        executor => executor.ExecuteScheduledAsync(job.Id),
                        job.RunAt.Value.UtcDateTime - DateTime.UtcNow);
                }

                return Task.FromResult<string?>(schedulerJobId);
            default:
                return Task.FromResult<string?>(schedulerJobId);
        }
    }
    /// <summary>
    /// Update schedule async.
    /// </summary>

    public Task<string?> UpdateScheduleAsync(Job job, CancellationToken cancellationToken)
        => ScheduleJobAsync(job, cancellationToken);
    /// <summary>
    /// Delete job async.
    /// </summary>

    public Task DeleteJobAsync(Job job, CancellationToken cancellationToken)
    {
        recurringJobManager.RemoveIfExists(BuildRecurringJobId(job));
        return Task.CompletedTask;
    }
    /// <summary>
    /// Trigger job async.
    /// </summary>

    public async Task<Guid> TriggerJobAsync(Job job, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var execution = await jobExecutionService.CreatePendingExecutionAsync(job.Id, triggeredBy, retryParentId, cancellationToken);
        backgroundJobClient.Enqueue<JobExecutor>(executor => executor.ExecuteQueuedAsync(job.Id, execution.Id, triggeredBy, retryParentId));
        return execution.Id;
    }
    /// <summary>
    /// Pause job async.
    /// </summary>

    public Task PauseJobAsync(Job job, CancellationToken cancellationToken)
    {
        recurringJobManager.RemoveIfExists(BuildRecurringJobId(job));
        return Task.CompletedTask;
    }
    /// <summary>
    /// Resume job async.
    /// </summary>

    public Task ResumeJobAsync(Job job, CancellationToken cancellationToken)
        => ScheduleJobAsync(job, cancellationToken);
    /// <summary>
    /// Get job status async.
    /// </summary>

    public Task<string> GetJobStatusAsync(Job job, CancellationToken cancellationToken)
    {
        if (job.IsDeleted)
        {
            return Task.FromResult("Deleted");
        }

        if (job.IsPaused)
        {
            return Task.FromResult("Paused");
        }

        if (!job.IsEnabled)
        {
            return Task.FromResult("Disabled");
        }

        if (job.LastExecutionStatus == ExecutionStatus.Failed)
        {
            return Task.FromResult("Failed");
        }

        return Task.FromResult("Active");
    }
    /// <summary>
    /// Get upcoming executions async.
    /// </summary>

    public async Task<IReadOnlyList<UpcomingExecutionDto>> GetUpcomingExecutionsAsync(int take, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetUpcomingJobsAsync(take, cancellationToken);
        return jobs
            .Select(job => new UpcomingExecutionDto(job.Id, job.Name, EstimateNextRun(job)))
            .OrderBy(x => x.NextRun ?? DateTimeOffset.MaxValue)
            .Take(take)
            .ToArray();
    }
    /// <summary>
    /// Build recurring job id.
    /// </summary>

    private static string BuildRecurringJobId(Job job) => $"scheduler-job:{job.Id}";
    /// <summary>
    /// Build interval cron.
    /// </summary>

    private static string BuildIntervalCron(int intervalSeconds)
    {
        var minutes = Math.Max(5, intervalSeconds / 60);
        return minutes >= 60
            ? $"0 */{Math.Max(1, minutes / 60)} * * *"
            : $"*/{minutes} * * * *";
    }
    /// <summary>
    /// Estimate next run.
    /// </summary>

    private static DateTimeOffset? EstimateNextRun(Job job)
    {
        return job.ScheduleType switch
        {
            ScheduleType.OneTime => job.RunAt,
            ScheduleType.Interval when job.IntervalSeconds.HasValue => DateTimeOffset.UtcNow.AddSeconds(job.IntervalSeconds.Value),
            ScheduleType.Cron => DateTimeOffset.UtcNow.AddMinutes(5),
            _ => null
        };
    }
}
