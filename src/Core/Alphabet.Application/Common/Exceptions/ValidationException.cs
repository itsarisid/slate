namespace Alphabet.Application.Common.Exceptions;

/// <summary>
/// Represents a validation exception raised by the application layer.
/// </summary>
public sealed class ValidationException(IDictionary<string, string[]> errors) : Exception("One or more validation failures occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
