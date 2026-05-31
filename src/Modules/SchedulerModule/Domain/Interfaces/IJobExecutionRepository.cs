using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Repository contract for job execution records.
/// </summary>
public interface IJobExecutionRepository
{
    /// <summary>
    /// Get by id async.
    /// </summary>
    Task<JobExecution?> GetByIdAsync(Guid executionId, CancellationToken cancellationToken);
    /// <summary>
    /// Add async.
    /// </summary>

    Task AddAsync(JobExecution execution, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(JobExecution execution);
    /// <summary>
    /// Get paged by job id async.
    /// </summary>

    Task<PagedResult<JobExecution>> GetPagedByJobIdAsync(JobExecutionQueryFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get dashboard stats async.
    /// </summary>

    Task<DashboardStatsSnapshot> GetDashboardStatsAsync(DateTimeOffset fromDate, CancellationToken cancellationToken);
    /// <summary>
    /// Get timeline async.
    /// </summary>

    Task<IReadOnlyList<TimelinePoint>> GetTimelineAsync(DateTimeOffset fromDate, CancellationToken cancellationToken);
    /// <summary>
    /// Clear older than async.
    /// </summary>

    Task<int> ClearOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
    /// <summary>
    /// Has successful execution async.
    /// </summary>

    Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken);
}
