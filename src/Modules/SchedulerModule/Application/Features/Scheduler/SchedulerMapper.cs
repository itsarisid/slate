using System.Text.Json;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Application.Features.Scheduler;

/// <summary>
/// Maps scheduler entities to API-facing DTOs.
/// </summary>
public static class SchedulerMapper
{
    public static JobDto ToDto(this Job job, string currentStatus)
    {
        var config = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration) ?? new JobConfigurationDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var retry = JsonSerializer.Deserialize<RetryPolicyDto>(job.RetryPolicy) ?? new RetryPolicyDto(0, 0, Domain.Enums.RetryBackoffType.Fixed, null, null);
        var tags = JsonSerializer.Deserialize<IReadOnlyList<string>>(job.Tags) ?? [];

        return new JobDto(
            job.Id,
            job.Name,
            job.Description,
            job.JobType,
            job.ScheduleType,
            job.ScheduleExpression,
            job.IntervalSeconds,
            job.RunAt,
            config,
            retry,
            job.Timezone,
            job.IsEnabled,
            job.IsPaused,
            job.IsDeleted,
            job.CreatedBy,
            job.LastModifiedBy,
            tags,
            job.SchedulerJobId,
            currentStatus,
            job.CreatedAt,
            job.UpdatedAt,
            job.LastExecutedAt,
            job.LastExecutionStatus,
            job.ConsecutiveFailures);
    }

    public static JobExecutionDto ToDto(this JobExecution execution)
        => new(
            execution.Id,
            execution.JobId,
            execution.TriggeredBy,
            execution.StartedAt,
            execution.EndedAt,
            execution.Status,
            execution.DurationMs,
            execution.Output,
            execution.ErrorMessage,
            execution.RetryCount,
            execution.RetryParentId,
            execution.CreatedAt);

    public static PagedResponseDto<JobDto> ToDto(this PagedResult<Job> jobs, IReadOnlyDictionary<Guid, string> statuses)
        => new(
            jobs.Items.Select(job => job.ToDto(statuses.TryGetValue(job.Id, out var status) ? status : "Unknown")).ToArray(),
            jobs.TotalCount,
            jobs.PageNumber,
            jobs.PageSize);

    public static PagedResponseDto<JobExecutionDto> ToDto(this PagedResult<JobExecution> executions)
        => new(
            executions.Items.Select(ToDto).ToArray(),
            executions.TotalCount,
            executions.PageNumber,
            executions.PageSize);
}
