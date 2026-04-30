namespace Alphabet.Domain.Models;

/// <summary>
/// Execution timeline bucket.
/// </summary>
public sealed record TimelinePoint(DateTimeOffset Bucket, int SuccessCount, int FailedCount, int RunningCount);
