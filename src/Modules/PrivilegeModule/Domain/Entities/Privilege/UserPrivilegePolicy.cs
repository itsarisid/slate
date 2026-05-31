namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a privilege policy assigned directly to a user.
/// </summary>
public sealed class UserPrivilegePolicy : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid PolicyId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string GrantedBy { get; private set; } = "system";

    public DateTimeOffset? ExpiresAt { get; private set; }
    /// <summary>
    /// Create.
    /// </summary>

    public static UserPrivilegePolicy Create(Guid userId, Guid policyId, string grantedBy, DateTimeOffset? expiresAt)
    {
        return new UserPrivilegePolicy
        {
            UserId = userId,
            PolicyId = policyId,
            GrantedBy = string.IsNullOrWhiteSpace(grantedBy) ? "system" : grantedBy.Trim(),
            ExpiresAt = expiresAt
        };
    }
}
