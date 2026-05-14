using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides task persistence operations.
/// </summary>
public interface ITaskRepository
{
    Task AddAsync(ProductivityTask task, CancellationToken cancellationToken);

    Task<ProductivityTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductivityTask>> GetBoardAsync(TaskBoardFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(Guid taskId, CancellationToken cancellationToken);

    void Update(ProductivityTask task);
}
