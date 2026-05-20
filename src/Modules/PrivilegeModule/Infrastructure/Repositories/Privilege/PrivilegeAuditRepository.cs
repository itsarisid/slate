using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories.Privilege;

/// <summary>
/// EF Core repository for privilege audit records.
/// </summary>
public sealed class PrivilegeAuditRepository(AppDbContext dbContext) : IPrivilegeAuditRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    public Task AddAsync(PrivilegeAuditLog logEntry, CancellationToken cancellationToken)
    => dbContext.Set<PrivilegeAuditLog>().AddAsync(logEntry, cancellationToken).AsTask();
    /// <summary>
    /// Search async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegeAuditLog>> SearchAsync(
        Guid? userId,
        Guid? privilegeId,
        PrivilegeAction? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int take,
        int skip,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<PrivilegeAuditLog>().AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (privilegeId.HasValue)
        {
            query = query.Where(x => x.PrivilegeId == privilegeId.Value);
        }

        if (action.HasValue)
        {
            query = query.Where(x => x.Action == action.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.PerformedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.PerformedAt <= to.Value);
        }

        return await query
            .OrderByDescending(x => x.PerformedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get by user async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegeAuditLog>> GetByUserAsync(Guid userId, int take, int skip, CancellationToken cancellationToken)
        => await dbContext.Set<PrivilegeAuditLog>()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.PerformedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
}
