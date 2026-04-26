namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a new user is registered.
/// </summary>
public sealed record UserRegisteredEvent(Guid UserId, string Email);
