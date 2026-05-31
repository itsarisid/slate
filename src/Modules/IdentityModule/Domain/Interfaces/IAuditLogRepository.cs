using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Stores and queries authentication audit entries.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
    /// <summary>
    /// Get by user id async.
    /// </summary>

    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, int take, int skip, CancellationToken cancellationToken);
    /// <summary>
    /// Get all async.
    /// </summary>

    Task<IReadOnlyList<AuditLog>> GetAllAsync(int take, int skip, CancellationToken cancellationToken);
}
