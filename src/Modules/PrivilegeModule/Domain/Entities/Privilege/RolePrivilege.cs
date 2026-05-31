namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a privilege granted to a role.
/// </summary>
public sealed class RolePrivilege : BaseEntity
{
    public Guid RoleId { get; private set; }

    public Guid PrivilegeId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string GrantedBy { get; private set; } = "system";

    public DateTimeOffset? ExpiresAt { get; private set; }

    public bool IsActive { get; private set; } = true;
    /// <summary>
    /// Create.
    /// </summary>

    public static RolePrivilege Create(Guid roleId, Guid privilegeId, string grantedBy, DateTimeOffset? expiresAt)
    {
        return new RolePrivilege
        {
            RoleId = roleId,
            PrivilegeId = privilegeId,
            GrantedBy = string.IsNullOrWhiteSpace(grantedBy) ? "system" : grantedBy.Trim(),
            ExpiresAt = expiresAt
        };
    }
    /// <summary>
    /// Revoke.
    /// </summary>

    public void Revoke()
    {
        IsActive = false;
        Touch();
    }
}
