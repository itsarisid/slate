namespace Alphabet.Domain.Models;

/// <summary>
/// Snapshot used to build scheduler dashboard statistics.
/// </summary>
public sealed record DashboardStatsSnapshot(
    int TotalJobs,
    int ActiveJobs,
    int PausedJobs,
    int FailedJobs,
    int ExecutionsToday,
    double SuccessRate,
    double AverageDurationSeconds);
