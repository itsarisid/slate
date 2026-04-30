namespace Alphabet.Domain.Enums;

/// <summary>
/// Retry backoff strategies.
/// </summary>
public enum RetryBackoffType
{
    Fixed = 1,
    Linear = 2,
    Exponential = 3
}
