namespace Alphabet.Domain.Exceptions;

/// <summary>
/// Represents an unrecoverable domain invariant violation.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
