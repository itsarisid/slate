using Alphabet.Domain.Enums;

namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Retry behavior for a scheduled job.
/// </summary>
public sealed record RetryPolicyDto(
    int MaxRetryAttempts,
    int RetryDelaySeconds,
    RetryBackoffType RetryBackoffType,
    IReadOnlyList<string>? RetryOnExceptions,
    IReadOnlyList<int>? ShouldRetryHttpStatusCodes);
