using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Repository contract for job execution records.
/// </summary>
public interface IJobExecutionRepository
{
    Task<JobExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken);

    Task AddAsync(JobExecution execution, CancellationToken cancellationToken);

    void Update(JobExecution execution);

    Task<PagedResult<JobExecution>> GetPagedByJobIdAsync(JobExecutionQueryFilter filter, CancellationToken cancellationToken);

    Task<DashboardStatsSnapshot> GetDashboardStatsAsync(DateTimeOffset fromDate, CancellationToken cancellationToken);

    Task<IReadOnlyList<TimelinePoint>> GetTimelineAsync(DateTimeOffset fromDate, CancellationToken cancellationToken);

    Task<int> ClearOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);

    Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken);
}
