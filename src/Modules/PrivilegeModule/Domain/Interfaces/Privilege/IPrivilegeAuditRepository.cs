using Alphabet.Domain.Entities.Privilege;

namespace Alphabet.Domain.Interfaces.Privilege;

/// <summary>
/// Provides persistence access for privilege audit records.
/// </summary>
public interface IPrivilegeAuditRepository
{
    Task AddAsync(PrivilegeAuditLog logEntry, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeAuditLog>> SearchAsync(
        Guid? userId,
        Guid? privilegeId,
        Alphabet.Domain.Enums.PrivilegeAction? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int take,
        int skip,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeAuditLog>> GetByUserAsync(
        Guid userId,
        int take,
        int skip,
        CancellationToken cancellationToken);
}
