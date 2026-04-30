using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for job executions.
/// </summary>
public sealed class JobExecutionRepository(AppDbContext dbContext) : IJobExecutionRepository
{
    public Task<JobExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken)
        => dbContext.Set<JobExecution>().FirstOrDefaultAsync(x => x.Id == executionId, cancellationToken);

    public async Task AddAsync(JobExecution execution, CancellationToken cancellationToken)
        => await dbContext.Set<JobExecution>().AddAsync(execution, cancellationToken);

    public void Update(JobExecution execution) => dbContext.Set<JobExecution>().Update(execution);

    public async Task<PagedResult<JobExecution>> GetPagedByJobIdAsync(JobExecutionQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<JobExecution>().Where(x => x.JobId == filter.JobId);

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.StartedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.StartedAt <= filter.ToDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobExecution>(items, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<DashboardStatsSnapshot> GetDashboardStatsAsync(DateTimeOffset fromDate, CancellationToken cancellationToken)
    {
        var jobsQuery = dbContext.Set<Job>();
        var executionsToday = dbContext.Set<JobExecution>().Where(x => x.StartedAt >= fromDate);

        var totalJobs = await jobsQuery.CountAsync(cancellationToken);
        var activeJobs = await jobsQuery.CountAsync(x => x.IsEnabled && !x.IsPaused && !x.IsDeleted, cancellationToken);
        var pausedJobs = await jobsQuery.CountAsync(x => x.IsPaused && !x.IsDeleted, cancellationToken);
        var failedJobs = await jobsQuery.CountAsync(x => x.ConsecutiveFailures > 0 && !x.IsDeleted, cancellationToken);
        var executionsTodayCount = await executionsToday.CountAsync(cancellationToken);
        var successExecutions = await executionsToday.CountAsync(x => x.Status == ExecutionStatus.Success, cancellationToken);
        var averageDurationMs = await executionsToday
            .Where(x => x.DurationMs.HasValue)
            .Select(x => (double?)x.DurationMs)
            .AverageAsync(cancellationToken) ?? 0d;

        var successRate = executionsTodayCount == 0 ? 100d : (double)successExecutions / executionsTodayCount * 100d;

        return new DashboardStatsSnapshot(
            totalJobs,
            activeJobs,
            pausedJobs,
            failedJobs,
            executionsTodayCount,
            Math.Round(successRate, 2),
            Math.Round(averageDurationMs / 1000d, 2));
    }

    public async Task<IReadOnlyList<TimelinePoint>> GetTimelineAsync(DateTimeOffset fromDate, CancellationToken cancellationToken)
    {
        var executions = await dbContext.Set<JobExecution>()
            .Where(x => x.StartedAt >= fromDate)
            .ToListAsync(cancellationToken);

        return executions
            .GroupBy(x => new DateTimeOffset(x.StartedAt.Year, x.StartedAt.Month, x.StartedAt.Day, x.StartedAt.Hour, 0, 0, TimeSpan.Zero))
            .OrderBy(group => group.Key)
            .Select(group => new TimelinePoint(
                group.Key,
                group.Count(x => x.Status == ExecutionStatus.Success),
                group.Count(x => x.Status == ExecutionStatus.Failed || x.Status == ExecutionStatus.TimedOut),
                group.Count(x => x.Status == ExecutionStatus.Running)))
            .ToArray();
    }

    public Task<int> ClearOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
        => dbContext.Set<JobExecution>().Where(x => x.CreatedAt < cutoff).ExecuteDeleteAsync(cancellationToken);

    public Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken)
        => dbContext.Set<JobExecution>().AnyAsync(x => x.JobId == jobId && x.Status == ExecutionStatus.Success, cancellationToken);
}
