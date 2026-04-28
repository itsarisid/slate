using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Stores and queries authentication audit entries.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, int take, int skip, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditLog>> GetAllAsync(int take, int skip, CancellationToken cancellationToken);
}
