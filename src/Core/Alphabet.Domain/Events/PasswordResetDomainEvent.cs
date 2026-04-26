namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a user password has been reset.
/// </summary>
public sealed record PasswordResetDomainEvent(Guid UserId, string Email);
