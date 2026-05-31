using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Identity;

/// <summary>
/// Persists and queries audit-log entries.
/// </summary>
public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
    /// <summary>
    /// Get by user id async.
    /// </summary>

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, int take, int skip, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Timestamp)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get all async.
    /// </summary>

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(int take, int skip, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
