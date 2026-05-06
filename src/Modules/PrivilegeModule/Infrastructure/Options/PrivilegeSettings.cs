namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configuration for privilege evaluation, caching, and seeding.
/// </summary>
public sealed class PrivilegeSettings
{
    public const string SectionName = "PrivilegeSettings";

    public bool CacheEnabled { get; init; } = true;

    public int CacheDurationMinutes { get; init; } = 5;

    public string CacheProvider { get; init; } = "Memory";

    public string? RedisConnectionString { get; init; }

    public bool EnableAuditLogging { get; init; } = true;

    public bool EnablePrivilegeAnalytics { get; init; } = true;

    public int? DefaultPrivilegeExpiryDays { get; init; }

    public int MaxPrivilegeRequestDurationDays { get; init; } = 30;

    public bool RequireApprovalForNewPrivileges { get; init; } = false;

    public string[] AdminRoles { get; init; } = ["Admin", "PrivilegeManager"];

    public PrivilegeSeedItem[] SystemPrivileges { get; init; } = [];
}

/// <summary>
/// Represents a configured system privilege to seed.
/// </summary>
public sealed class PrivilegeSeedItem
{
    public string Name { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Category { get; init; } = "System";

    public bool IsGlobal { get; init; }
}
