namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Next scheduled execution preview.
/// </summary>
public sealed record UpcomingExecutionDto(Guid JobId, string JobName, DateTimeOffset? NextRun);
