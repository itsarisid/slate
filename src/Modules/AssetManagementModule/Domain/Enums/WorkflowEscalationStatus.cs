namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents workflow step execution status.
/// </summary>
public enum WorkflowEscalationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Delegated = 3,
    Completed = 4,
    Escalated = 5
}
