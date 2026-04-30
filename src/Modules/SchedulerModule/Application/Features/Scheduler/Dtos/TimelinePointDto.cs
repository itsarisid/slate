namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Timeline chart point for scheduler monitoring.
/// </summary>
public sealed record TimelinePointDto(DateTimeOffset Bucket, int SuccessCount, int FailedCount, int RunningCount);
