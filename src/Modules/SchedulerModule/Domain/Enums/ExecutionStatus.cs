namespace Alphabet.Domain.Enums;

/// <summary>
/// Execution lifecycle states.
/// </summary>
public enum ExecutionStatus
{
    Pending = 1,
    Running = 2,
    Success = 3,
    Failed = 4,
    Cancelled = 5,
    TimedOut = 6
}
