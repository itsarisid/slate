namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Caching settings.
/// </summary>
public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    public string Provider { get; init; } = "Memory";

    public string? RedisConnectionString { get; init; }

    public int DefaultExpirationMinutes { get; init; } = 5;
}
