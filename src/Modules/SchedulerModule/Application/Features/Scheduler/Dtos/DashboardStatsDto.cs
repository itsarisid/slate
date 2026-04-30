namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Scheduler dashboard summary.
/// </summary>
public sealed record DashboardStatsDto(
    int TotalJobs,
    int ActiveJobs,
    int PausedJobs,
    int FailedJobs,
    int ExecutionsToday,
    double SuccessRate,
    double AverageDurationSeconds,
    IReadOnlyList<UpcomingExecutionDto> UpcomingExecutions);
