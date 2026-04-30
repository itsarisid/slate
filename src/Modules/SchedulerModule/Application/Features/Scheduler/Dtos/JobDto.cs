using Alphabet.Domain.Enums;

namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Job projection returned by scheduler endpoints.
/// </summary>
public sealed record JobDto(
    Guid Id,
    string Name,
    string Description,
    JobType JobType,
    ScheduleType ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    JobConfigurationDto JobConfiguration,
    RetryPolicyDto RetryPolicy,
    string Timezone,
    bool IsEnabled,
    bool IsPaused,
    bool IsDeleted,
    string CreatedBy,
    string LastModifiedBy,
    IReadOnlyList<string> Tags,
    string? SchedulerJobId,
    string CurrentStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastExecutedAt,
    ExecutionStatus? LastExecutionStatus,
    int ConsecutiveFailures);
