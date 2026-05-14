namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a todo is completed.
/// </summary>
public sealed record TodoCompletedDomainEvent(Guid TodoId, Guid UserId, DateTimeOffset CompletedAt);
