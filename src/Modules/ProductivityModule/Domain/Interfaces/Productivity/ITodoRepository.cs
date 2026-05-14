using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides todo persistence operations.
/// </summary>
public interface ITodoRepository
{
    Task AddAsync(Todo todo, CancellationToken cancellationToken);

    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductivityPagedResult<Todo>> SearchAsync(TodoQueryFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<Todo>> GetTodayAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);

    void Update(Todo todo);
}
