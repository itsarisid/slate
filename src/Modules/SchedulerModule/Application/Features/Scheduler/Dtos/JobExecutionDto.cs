using Alphabet.Domain.Enums;

namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Execution projection returned by scheduler endpoints.
/// </summary>
public sealed record JobExecutionDto(
    Guid Id,
    Guid JobId,
    Guid? TriggeredBy,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    ExecutionStatus Status,
    long? DurationMs,
    string? Output,
    string? ErrorMessage,
    int RetryCount,
    Guid? RetryParentId,
    DateTimeOffset CreatedAt);
