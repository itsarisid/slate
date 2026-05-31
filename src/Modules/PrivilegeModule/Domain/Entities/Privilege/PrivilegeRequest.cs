using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a self-service privilege access request.
/// </summary>
public sealed class PrivilegeRequest : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid PrivilegeId { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public int RequestedDurationDays { get; private set; }

    public PrivilegeRequestStatus Status { get; private set; } = PrivilegeRequestStatus.Pending;

    public Guid? ApproverId { get; private set; }

    public DateTimeOffset? ApprovedAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public string? ApproverEmail { get; private set; }

    public string? DecisionNotes { get; private set; }
    /// <summary>
    /// Create.
    /// </summary>

    public static PrivilegeRequest Create(
        Guid userId,
        Guid privilegeId,
        string reason,
        int requestedDurationDays,
        string? approverEmail)
    {
        return new PrivilegeRequest
        {
            UserId = userId,
            PrivilegeId = privilegeId,
            Reason = reason.Trim(),
            RequestedDurationDays = requestedDurationDays,
            ApproverEmail = approverEmail?.Trim()
        };
    }
    /// <summary>
    /// Approve.
    /// </summary>

    public void Approve(Guid approverId, int durationDays, string? notes)
    {
        Status = PrivilegeRequestStatus.Approved;
        ApproverId = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        ExpiresAt = durationDays <= 0 ? null : DateTimeOffset.UtcNow.AddDays(durationDays);
        DecisionNotes = notes?.Trim();
        Touch();
    }
    /// <summary>
    /// Deny.
    /// </summary>

    public void Deny(Guid approverId, string? notes)
    {
        Status = PrivilegeRequestStatus.Denied;
        ApproverId = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        DecisionNotes = notes?.Trim();
        Touch();
    }
}
