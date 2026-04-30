using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Repository contract for scheduler jobs.
/// </summary>
public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Job job, CancellationToken cancellationToken);

    void Update(Job job);

    void Remove(Job job);

    Task<PagedResult<Job>> GetPagedAsync(JobQueryFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Job>> GetFailedJobsAsync(int threshold, CancellationToken cancellationToken);

    Task<IReadOnlyList<Job>> GetUpcomingJobsAsync(int take, CancellationToken cancellationToken);
}
