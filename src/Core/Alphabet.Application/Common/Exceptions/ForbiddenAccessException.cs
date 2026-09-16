namespace Alphabet.Application.Common.Exceptions;

/// <summary>
/// Raised when the current user cannot access an application resource.
/// </summary>
public sealed class ForbiddenAccessException(string message = "Forbidden access.") : Exception(message);
