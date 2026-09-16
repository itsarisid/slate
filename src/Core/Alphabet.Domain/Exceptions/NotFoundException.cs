namespace Alphabet.Domain.Exceptions;

/// <summary>
/// Raised when a requested domain resource cannot be found.
/// </summary>
public sealed class NotFoundException(string name, object key)
    : DomainException($"{name} with key '{key}' was not found.");
