using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an approval workflow instance for a leave request.
/// </summary>
public sealed class ApprovalWorkflow : BaseEntity
{
    private ApprovalWorkflow()
    {
    }

    private ApprovalWorkflow(Guid leaveRequestId, Guid approvalChainId, int currentLevel)
    {
        LeaveRequestId = leaveRequestId;
        ApprovalChainId = approvalChainId;
        Status = LeaveWorkflowStatus.Active;
        CurrentLevel = currentLevel;
        InitiatedAt = DateTimeOffset.UtcNow;
    }

    public Guid LeaveRequestId { get; private set; }

    public Guid ApprovalChainId { get; private set; }

    public LeaveWorkflowStatus Status { get; private set; }

    public int CurrentLevel { get; private set; }

    public DateTimeOffset InitiatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Creates a workflow instance.
    /// </summary>
    public static ApprovalWorkflow Create(Guid leaveRequestId, Guid approvalChainId, int currentLevel = 1)
    {
        return new ApprovalWorkflow(leaveRequestId, approvalChainId, currentLevel);
    }

    /// <summary>
    /// Advances to the next level.
    /// </summary>
    public void AdvanceTo(int level)
    {
        CurrentLevel = level;
        Touch();
    }

    /// <summary>
    /// Completes the workflow.
    /// </summary>
    public void Complete()
    {
        Status = LeaveWorkflowStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Rejects the workflow.
    /// </summary>
    public void Reject()
    {
        Status = LeaveWorkflowStatus.Rejected;
        CompletedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Escalates the workflow.
    /// </summary>
    public void Escalate()
    {
        Status = LeaveWorkflowStatus.Escalated;
        Touch();
    }
}
