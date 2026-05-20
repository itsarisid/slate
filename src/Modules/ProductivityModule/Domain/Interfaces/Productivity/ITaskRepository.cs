using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides task persistence operations.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(ProductivityTask task, CancellationToken cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    Task<ProductivityTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Get board async.
    /// </summary>

    Task<IReadOnlyList<ProductivityTask>> GetBoardAsync(TaskBoardFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get dependencies async.
    /// </summary>

    Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(Guid taskId, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(ProductivityTask task);
}
