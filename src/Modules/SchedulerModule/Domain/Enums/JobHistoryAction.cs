namespace Alphabet.Domain.Enums;

/// <summary>
/// Audited scheduler actions.
/// </summary>
public enum JobHistoryAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Rescheduled = 4,
    Paused = 5,
    Resumed = 6,
    Triggered = 7,
    Imported = 8
}
