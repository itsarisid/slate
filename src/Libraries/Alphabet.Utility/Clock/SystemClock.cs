namespace Alphabet.Utility.Clock;

/// <summary>
/// Provides system time access.
/// </summary>
public sealed class SystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
