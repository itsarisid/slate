using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents one approval step in a leave workflow.
/// </summary>
public sealed class WorkflowStep : BaseEntity
{
    private WorkflowStep()
    {
    }

    private WorkflowStep(Guid workflowId, int level, Guid? approverUserId, LeaveApproverType approverType, string approverValue, int timeoutHours)
    {
        WorkflowId = workflowId;
        Level = level;
        ApproverUserId = approverUserId;
        ApproverType = approverType;
        ApproverValue = approverValue;
        Status = LeaveWorkflowStepStatus.Pending;
        AssignedAt = DateTimeOffset.UtcNow;
        TimeoutHours = timeoutHours;
    }

    public Guid WorkflowId { get; private set; }

    public int Level { get; private set; }

    public Guid? ApproverUserId { get; private set; }

    public LeaveApproverType ApproverType { get; private set; }

    public string ApproverValue { get; private set; } = string.Empty;

    public LeaveWorkflowStepStatus Status { get; private set; }

    public string? Action { get; private set; }

    public string? Comment { get; private set; }

    public string AttachmentsJson { get; private set; } = "[]";

    public DateTimeOffset AssignedAt { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    public int TimeoutHours { get; private set; }

    public bool IsEscalated { get; private set; }

    /// <summary>
    /// Creates a workflow step.
    /// </summary>
    public static WorkflowStep Create(Guid workflowId, int level, Guid? approverUserId, LeaveApproverType approverType, string approverValue, int timeoutHours)
    {
        return new WorkflowStep(workflowId, level, approverUserId, approverType, approverValue, timeoutHours);
    }

    /// <summary>
    /// Records an approval decision.
    /// </summary>
    public void Approve(string? comment, IReadOnlyCollection<string> attachments)
    {
        Status = LeaveWorkflowStepStatus.Approved;
        Action = "Approve";
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        AttachmentsJson = LeaveManagementJson.Serialize(attachments);
        RespondedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Records a rejection decision.
    /// </summary>
    public void Reject(string reason)
    {
        Status = LeaveWorkflowStepStatus.Rejected;
        Action = "Reject";
        Comment = reason.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Records a request-changes decision.
    /// </summary>
    public void RequestChanges(string comment)
    {
        Status = LeaveWorkflowStepStatus.ChangesRequested;
        Action = "RequestChanges";
        Comment = comment.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Marks the step as escalated.
    /// </summary>
    public void Escalate()
    {
        IsEscalated = true;
        Status = LeaveWorkflowStepStatus.Escalated;
        Touch();
    }
}
