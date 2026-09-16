namespace Alphabet.Application.Results;

/// <summary>
/// Represents a stable application error.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
