namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents supported workflow step actions.
/// </summary>
public enum WorkflowStepAction
{
    Approve = 0,
    Reject = 1,
    RequestChanges = 2,
    Complete = 3,
    Delegate = 4
}
