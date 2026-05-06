using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents an audit event in the privilege subsystem.
/// </summary>
public sealed class PrivilegeAuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }

    public Guid? PrivilegeId { get; private set; }

    public PrivilegeAction Action { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string PerformedBy { get; private set; } = "system";

    public DateTimeOffset PerformedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string? IpAddress { get; private set; }

    public IDictionary<string, string?> Metadata { get; private set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public static PrivilegeAuditLog Create(
        Guid? userId,
        Guid? privilegeId,
        PrivilegeAction action,
        string source,
        string performedBy,
        string? ipAddress,
        IReadOnlyDictionary<string, string?>? metadata)
    {
        return new PrivilegeAuditLog
        {
            UserId = userId,
            PrivilegeId = privilegeId,
            Action = action,
            Source = source.Trim(),
            PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim(),
            IpAddress = ipAddress?.Trim(),
            Metadata = metadata is null
                ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string?>(metadata, StringComparer.OrdinalIgnoreCase)
        };
    }
}
