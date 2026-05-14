namespace Alphabet.Domain.Enums;

/// <summary>
/// Defines the current state of a todo item.
/// </summary>
public enum TodoStatus
{
    Pending = 0,
    Completed = 1,
    Archived = 2,
    Overdue = 3,
    Trash = 4
}
