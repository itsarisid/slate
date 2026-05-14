namespace Alphabet.Domain.Enums;

/// <summary>
/// Defines lifecycle states for productivity tasks.
/// </summary>
public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Blocked = 2,
    Completed = 3,
    Cancelled = 4
}
