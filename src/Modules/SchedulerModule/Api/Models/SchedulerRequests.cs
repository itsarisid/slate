using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Enums;

namespace Alphabet.Modules.SchedulerModule.Api.Models;

/// <summary>
/// Request payload for creating a scheduler job.
/// </summary>
public sealed record CreateSchedulerJobRequest(
    string Name,
    string Description,
    JobType JobType,
    ScheduleType ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    JobConfigurationDto JobConfiguration,
    RetryPolicyDto RetryPolicy,
    int? TimeoutSeconds,
    string Timezone,
    bool IsEnabled,
    IReadOnlyList<string>? Tags,
    string? CreatedBy);

/// <summary>
/// Request payload for updating a scheduler job.
/// </summary>
public sealed record UpdateSchedulerJobRequest(
    string? Name,
    string? Description,
    JobType? JobType,
    ScheduleType? ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    JobConfigurationDto? JobConfiguration,
    RetryPolicyDto? RetryPolicy,
    int? TimeoutSeconds,
    string? Timezone,
    bool? IsEnabled,
    IReadOnlyList<string>? Tags);

/// <summary>
/// Request payload for rescheduling a job.
/// </summary>
public sealed record RescheduleSchedulerJobRequest(
    ScheduleType ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    DateTimeOffset? EffectiveFrom);

/// <summary>
/// Request payload for exclusions.
/// </summary>
public sealed record AddJobExclusionRequest(
    IReadOnlyList<DateOnly>? ExcludedDates,
    IReadOnlyList<DayOfWeek>? ExcludedDaysOfWeek,
    TimeRangeDto? TimeRange);

/// <summary>
/// Excluded time range.
/// </summary>
public sealed record TimeRangeDto(string Start, string End);

/// <summary>
/// Request payload for job dependencies.
/// </summary>
public sealed record AddJobDependencyRequest(IReadOnlyList<Guid> DependsOnJobIds, string Condition);

/// <summary>
/// Request payload for workflow creation.
/// </summary>
public sealed record CreateWorkflowRequest(string Name, IReadOnlyList<CreateWorkflowJobRequest> Jobs);

/// <summary>
/// Workflow job item payload.
/// </summary>
public sealed record CreateWorkflowJobRequest(Guid JobId, IReadOnlyList<Guid> DependsOn, string OnFailure);

/// <summary>
/// Request payload for deleting old logs.
/// </summary>
public sealed record ClearExecutionLogsRequest(int OlderThanDays);

/// <summary>
/// Request payload for importing jobs.
/// </summary>
public sealed record ImportSchedulerJobsRequest(string JsonPayload);
