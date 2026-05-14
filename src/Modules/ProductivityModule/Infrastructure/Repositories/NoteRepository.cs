using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for notes.
/// </summary>
public sealed class NoteRepository(AppDbContext dbContext) : INoteRepository
{
    public async Task AddAsync(Note note, CancellationToken cancellationToken)
        => await dbContext.Set<Note>().AddAsync(note, cancellationToken);

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Set<Note>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ProductivityPagedResult<Note>> SearchAsync(NoteQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<Note>().AsNoTracking().Where(x => x.OwnerUserId == filter.OwnerUserId);

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (filter.NotebookId.HasValue)
        {
            query = query.Where(x => x.NotebookId == filter.NotebookId.Value);
        }

        if (filter.IsPinned.HasValue)
        {
            query = query.Where(x => x.IsPinned == filter.IsPinned.Value);
        }

        if (filter.IsFavorite.HasValue)
        {
            query = query.Where(x => x.IsFavorite == filter.IsFavorite.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => x.Title.Contains(filter.Search) || x.Content.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var taggedEntityIds = dbContext.Set<Tag>()
                .Where(x => x.EntityType == "Note" && x.NormalizedName == filter.Tag.ToLower())
                .Select(x => x.EntityId);

            query = query.Where(x => taggedEntityIds.Contains(x.Id));
        }

        query = query.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.UpdatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new ProductivityPagedResult<Note>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<Note>> GetRecentAsync(Guid ownerUserId, int take, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Note>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public void Update(Note note) => dbContext.Set<Note>().Update(note);
}
