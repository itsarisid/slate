using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides todo persistence operations.
/// </summary>
public interface ITodoRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(Todo todo, CancellationToken cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Search async.
    /// </summary>

    Task<ProductivityPagedResult<Todo>> SearchAsync(TodoQueryFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get today async.
    /// </summary>

    Task<IReadOnlyList<Todo>> GetTodayAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(Todo todo);
}
