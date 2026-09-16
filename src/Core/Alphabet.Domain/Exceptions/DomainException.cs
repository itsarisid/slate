namespace Alphabet.Domain.Exceptions;

/// <summary>
/// Represents an unrecoverable domain invariant violation.
/// </summary>
public class DomainException(string message) : Exception(message);
