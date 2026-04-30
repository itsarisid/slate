using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for scheduler jobs.
/// </summary>
public sealed class JobRepository(AppDbContext dbContext) : IJobRepository
{
    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Set<Job>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Job job, CancellationToken cancellationToken)
        => await dbContext.Set<Job>().AddAsync(job, cancellationToken);

    public void Update(Job job) => dbContext.Set<Job>().Update(job);

    public void Remove(Job job) => dbContext.Set<Job>().Remove(job);

    public async Task<PagedResult<Job>> GetPagedAsync(JobQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<Job>().AsQueryable();

        if (filter.JobType.HasValue)
        {
            query = query.Where(x => x.JobType == filter.JobType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.CreatedBy))
        {
            query = query.Where(x => x.CreatedBy == filter.CreatedBy);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            query = query.Where(x => x.Tags.Contains(filter.Tag));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => x.Name.Contains(filter.Search) || x.Description.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = filter.Status.Trim().ToLowerInvariant() switch
            {
                "active" => query.Where(x => x.IsEnabled && !x.IsPaused && !x.IsDeleted),
                "paused" => query.Where(x => x.IsPaused && !x.IsDeleted),
                "failed" => query.Where(x => x.ConsecutiveFailures > 0 && !x.IsDeleted),
                "completed" => query.Where(x => x.LastExecutionStatus == Domain.Enums.ExecutionStatus.Success && !x.IsDeleted),
                _ => query
            };
        }

        query = (filter.SortBy?.Trim().ToLowerInvariant(), filter.SortDirection?.Trim().ToLowerInvariant()) switch
        {
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("name", _) => query.OrderByDescending(x => x.Name),
            ("updatedat", "asc") => query.OrderBy(x => x.UpdatedAt),
            ("updatedat", _) => query.OrderByDescending(x => x.UpdatedAt),
            (_, "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Job>(items, total, filter.PageNumber, filter.PageSize);
    }

    public Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken)
        => dbContext.Set<Job>().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<Job>)t.Result, cancellationToken);

    public Task<IReadOnlyList<Job>> GetFailedJobsAsync(int threshold, CancellationToken cancellationToken)
        => dbContext.Set<Job>()
            .Where(x => !x.IsDeleted && x.ConsecutiveFailures >= threshold)
            .OrderByDescending(x => x.ConsecutiveFailures)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Job>)t.Result, cancellationToken);

    public Task<IReadOnlyList<Job>> GetUpcomingJobsAsync(int take, CancellationToken cancellationToken)
        => dbContext.Set<Job>()
            .Where(x => !x.IsDeleted && x.IsEnabled && !x.IsPaused)
            .OrderBy(x => x.RunAt ?? DateTimeOffset.MaxValue)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Job>)t.Result, cancellationToken);
}
