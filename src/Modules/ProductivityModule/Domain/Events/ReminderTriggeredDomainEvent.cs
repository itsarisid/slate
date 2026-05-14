namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a reminder should be delivered.
/// </summary>
public sealed record ReminderTriggeredDomainEvent(Guid ReminderId, Guid OwnerUserId, string Title);
