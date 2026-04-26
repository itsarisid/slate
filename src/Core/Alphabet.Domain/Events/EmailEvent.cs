namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when an email must be sent.
/// </summary>
public sealed record EmailEvent(string To, string Subject, string Body);
