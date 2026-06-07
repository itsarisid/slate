namespace Alphabet.Domain.Exceptions;

/// <summary>
/// Raised when a domain invariant or business rule is violated.
/// </summary>
public sealed class BusinessRuleViolationException(string message) : DomainException(message);
