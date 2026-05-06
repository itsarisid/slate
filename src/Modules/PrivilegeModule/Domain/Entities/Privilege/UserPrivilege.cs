using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a direct privilege assignment to a user.
/// </summary>
public sealed class UserPrivilege : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid PrivilegeId { get; private set; }

    public PrivilegeEffect Effect { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string GrantedBy { get; private set; } = "system";

    public DateTimeOffset? ExpiresAt { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? RevokedBy { get; private set; }

    public static UserPrivilege Create(
        Guid userId,
        Guid privilegeId,
        PrivilegeEffect effect,
        string grantedBy,
        DateTimeOffset? expiresAt,
        string? reason)
    {
        return new UserPrivilege
        {
            UserId = userId,
            PrivilegeId = privilegeId,
            Effect = effect,
            GrantedBy = string.IsNullOrWhiteSpace(grantedBy) ? "system" : grantedBy.Trim(),
            ExpiresAt = expiresAt,
            Reason = reason?.Trim()
        };
    }

    public bool IsActive(DateTimeOffset nowUtc) => RevokedAt is null && (!ExpiresAt.HasValue || ExpiresAt > nowUtc);

    public void Revoke(string revokedBy)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedBy = string.IsNullOrWhiteSpace(revokedBy) ? "system" : revokedBy.Trim();
        Touch();
    }
}
