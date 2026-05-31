using Alphabet.Domain.Entities.Privilege;

namespace Alphabet.Domain.Interfaces.Privilege;

/// <summary>
/// Provides persistence access for privilege audit records.
/// </summary>
public interface IPrivilegeAuditRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(PrivilegeAuditLog logEntry, CancellationToken cancellationToken);
    /// <summary>
    /// Search async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeAuditLog>> SearchAsync(
        Guid? userId,
        Guid? privilegeId,
        Alphabet.Domain.Enums.PrivilegeAction? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int take,
        int skip,
        CancellationToken cancellationToken);
    /// <summary>
    /// Get by user async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeAuditLog>> GetByUserAsync(
        Guid userId,
        int take,
        int skip,
        CancellationToken cancellationToken);
}
