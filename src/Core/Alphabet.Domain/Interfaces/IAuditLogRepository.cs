using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Stores authentication audit entries.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
}
