namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configures recurrence safety limits.
/// </summary>
public sealed class RecurrenceSettings
{
    public const string SectionName = "Recurrence";

    public int MaxFutureOccurrences { get; init; } = 100;

    public int MaxYearsRepeat { get; init; } = 5;
}
