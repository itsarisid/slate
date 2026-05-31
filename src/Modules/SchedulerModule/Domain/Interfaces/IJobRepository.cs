using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Repository contract for scheduler jobs.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Get by id async.
    /// </summary>
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Add async.
    /// </summary>

    Task AddAsync(Job job, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(Job job);
    /// <summary>
    /// Remove.
    /// </summary>

    void Remove(Job job);
    /// <summary>
    /// Get paged async.
    /// </summary>

    Task<PagedResult<Job>> GetPagedAsync(JobQueryFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get all async.
    /// </summary>

    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Get failed jobs async.
    /// </summary>

    Task<IReadOnlyList<Job>> GetFailedJobsAsync(int threshold, CancellationToken cancellationToken);
    /// <summary>
    /// Get upcoming jobs async.
    /// </summary>

    Task<IReadOnlyList<Job>> GetUpcomingJobsAsync(int take, CancellationToken cancellationToken);
}
