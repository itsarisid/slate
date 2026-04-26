namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when an out-of-band notification should be published.
/// </summary>
public sealed record NotificationEvent(string Channel, string Message);
