using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

/// <summary>
/// Returns dashboard statistics for the scheduler module.
/// </summary>
public sealed record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public sealed class GetDashboardStatsQueryHandler(
    IJobRepository jobRepository,
    IJobExecutionRepository executionRepository,
    ISchedulerService schedulerService) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await executionRepository.GetDashboardStatsAsync(DateTimeOffset.UtcNow.Date, cancellationToken);
        var upcoming = await schedulerService.GetUpcomingExecutionsAsync(10, cancellationToken);
        return new DashboardStatsDto(
            snapshot.TotalJobs,
            snapshot.ActiveJobs,
            snapshot.PausedJobs,
            snapshot.FailedJobs,
            snapshot.ExecutionsToday,
            snapshot.SuccessRate,
            snapshot.AverageDurationSeconds,
            upcoming);
    }
}
