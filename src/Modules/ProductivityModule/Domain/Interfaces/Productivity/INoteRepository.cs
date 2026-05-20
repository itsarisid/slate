using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides note persistence operations.
/// </summary>
public interface INoteRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(Note note, CancellationToken cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Search async.
    /// </summary>

    Task<ProductivityPagedResult<Note>> SearchAsync(NoteQueryFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get recent async.
    /// </summary>

    Task<IReadOnlyList<Note>> GetRecentAsync(Guid ownerUserId, int take, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(Note note);
}
