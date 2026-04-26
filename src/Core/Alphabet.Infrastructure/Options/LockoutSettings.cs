namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Lockout and failed-attempt settings.
/// </summary>
public sealed class LockoutSettings
{
    public const string SectionName = "LockoutSettings";

    public int MaxFailedAttempts { get; init; } = 5;

    public int LockoutDurationMinutes { get; init; } = 5;
}
