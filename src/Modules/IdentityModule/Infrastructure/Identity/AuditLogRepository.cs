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
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

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
