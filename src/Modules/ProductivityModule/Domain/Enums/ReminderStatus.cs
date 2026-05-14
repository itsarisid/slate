namespace Alphabet.Domain.Enums;

/// <summary>
/// Defines the current reminder delivery state.
/// </summary>
public enum ReminderStatus
{
    Active = 0,
    Triggered = 1,
    Snoozed = 2,
    Dismissed = 3,
    Completed = 4
}
