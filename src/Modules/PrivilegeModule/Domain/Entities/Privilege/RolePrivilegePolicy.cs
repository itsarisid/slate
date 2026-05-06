namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a privilege policy assigned to a role.
/// </summary>
public sealed class RolePrivilegePolicy : BaseEntity
{
    public Guid RoleId { get; private set; }

    public Guid PolicyId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string GrantedBy { get; private set; } = "system";

    public DateTimeOffset? ExpiresAt { get; private set; }

    public static RolePrivilegePolicy Create(Guid roleId, Guid policyId, string grantedBy, DateTimeOffset? expiresAt)
    {
        return new RolePrivilegePolicy
        {
            RoleId = roleId,
            PolicyId = policyId,
            GrantedBy = string.IsNullOrWhiteSpace(grantedBy) ? "system" : grantedBy.Trim(),
            ExpiresAt = expiresAt
        };
    }
}
