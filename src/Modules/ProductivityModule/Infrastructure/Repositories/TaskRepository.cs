using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for productivity tasks.
/// </summary>
public sealed class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    public async Task AddAsync(ProductivityTask task, CancellationToken cancellationToken)
    => await dbContext.Set<ProductivityTask>().AddAsync(task, cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    public async Task<ProductivityTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Set<ProductivityTask>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    /// <summary>
    /// Get board async.
    /// </summary>

    public async Task<IReadOnlyList<ProductivityTask>> GetBoardAsync(TaskBoardFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<ProductivityTask>().AsNoTracking();

        if (filter.OwnerUserId.HasValue)
        {
            query = query.Where(x => x.OwnerUserId == filter.OwnerUserId.Value || x.AssigneeId == filter.OwnerUserId.Value);
        }

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == filter.ProjectId.Value);
        }

        if (filter.AssigneeId.HasValue)
        {
            query = query.Where(x => x.AssigneeId == filter.AssigneeId.Value);
        }

        return await query.OrderBy(x => x.Status).ThenByDescending(x => x.Priority).ToArrayAsync(cancellationToken);
    }
    /// <summary>
    /// Get dependencies async.
    /// </summary>

    public async Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<TaskDependency>()
            .AsNoTracking()
            .Where(x => x.ProductivityTaskId == taskId)
            .ToArrayAsync(cancellationToken);
    }
    /// <summary>
    /// Update.
    /// </summary>

    public void Update(ProductivityTask task) => dbContext.Set<ProductivityTask>().Update(task);
}
