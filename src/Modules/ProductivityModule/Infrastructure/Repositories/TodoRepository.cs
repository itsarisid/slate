using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for todos.
/// </summary>
public sealed class TodoRepository(AppDbContext dbContext) : ITodoRepository
{
    public async Task AddAsync(Todo todo, CancellationToken cancellationToken)
        => await dbContext.Set<Todo>().AddAsync(todo, cancellationToken);

    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Set<Todo>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ProductivityPagedResult<Todo>> SearchAsync(TodoQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<Todo>().AsNoTracking().Where(x => !x.IsDeleted);

        if (filter.OwnerUserId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserId == filter.OwnerUserId.Value || x.AssignedToUserId == filter.OwnerUserId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (filter.AssignedTo.HasValue)
        {
            query = query.Where(x => x.AssignedToUserId == filter.AssignedTo.Value);
        }

        if (filter.DueDateFrom.HasValue)
        {
            query = query.Where(x => x.DueDate >= filter.DueDateFrom.Value);
        }

        if (filter.DueDateTo.HasValue)
        {
            query = query.Where(x => x.DueDate <= filter.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => x.Title.Contains(filter.Search) || x.Description.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var taggedEntityIds = dbContext.Set<Tag>()
                .Where(x => x.EntityType == "Todo" && x.NormalizedName == filter.Tag.ToLower())
                .Select(x => x.EntityId);

            query = query.Where(x => taggedEntityIds.Contains(x.Id));
        }

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "priority" => string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Priority)
                : query.OrderBy(x => x.Priority),
            _ => string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.DueDate).ThenByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new ProductivityPagedResult<Todo>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<Todo>> GetTodayAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Todo>()
            .AsNoTracking()
            .Where(x => (x.CreatedByUserId == ownerUserId || x.AssignedToUserId == ownerUserId) && x.DueDate >= start && x.DueDate <= end && !x.IsDeleted)
            .OrderBy(x => x.DueDate)
            .ToArrayAsync(cancellationToken);
    }

    public void Update(Todo todo) => dbContext.Set<Todo>().Update(todo);
}
