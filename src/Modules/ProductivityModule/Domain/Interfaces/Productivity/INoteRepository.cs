using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides note persistence operations.
/// </summary>
public interface INoteRepository
{
    Task AddAsync(Note note, CancellationToken cancellationToken);

    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductivityPagedResult<Note>> SearchAsync(NoteQueryFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<Note>> GetRecentAsync(Guid ownerUserId, int take, CancellationToken cancellationToken);

    void Update(Note note);
}
