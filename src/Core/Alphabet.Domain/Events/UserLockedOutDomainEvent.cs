namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a user is locked out after repeated failed attempts.
/// </summary>
public sealed record UserLockedOutDomainEvent(Guid UserId, string Email, DateTimeOffset LockoutEnd);
