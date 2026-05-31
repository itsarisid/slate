namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents workflow instance states.
/// </summary>
public enum WorkflowInstanceStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2,
    Escalated = 3
}
