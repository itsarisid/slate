using Alphabet.Domain.Enums;

namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Audit history projection for a job.
/// </summary>
public sealed record JobHistoryDto(
    Guid Id,
    Guid JobId,
    JobHistoryAction Action,
    string Changes,
    string PerformedBy,
    DateTimeOffset PerformedAt,
    string? IpAddress);
