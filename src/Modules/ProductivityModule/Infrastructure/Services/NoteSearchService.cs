using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Provides note full-text style searching over EF Core.
/// </summary>
public sealed class NoteSearchService(AppDbContext dbContext)
{
    /// <summary>
    /// Search async.
    /// </summary>
    public async Task<IReadOnlyList<Note>> SearchAsync(Guid ownerUserId, string query, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Note>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && (x.Title.Contains(query) || x.Content.Contains(query)))
            .OrderByDescending(x => x.UpdatedAt)
            .ToArrayAsync(cancellationToken);
    }
}
