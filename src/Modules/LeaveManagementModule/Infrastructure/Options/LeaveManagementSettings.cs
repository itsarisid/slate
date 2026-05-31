namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Provides configurable leave management options.
/// </summary>
public sealed class LeaveManagementSettings
{
    public const string SectionName = "LeaveManagement";

    public string DefaultCountry { get; init; } = "QA";

    public bool ExcludeWeekends { get; init; } = true;

    public bool ExcludePublicHolidays { get; init; } = true;

    public string[] WeekendDays { get; init; } = ["Friday", "Saturday"];

    public string[] NotificationChannels { get; init; } = ["InApp", "Email"];

    public LeaveManagementJobSettings Jobs { get; init; } = new();
}

/// <summary>
/// Provides leave background job configuration.
/// </summary>
public sealed class LeaveManagementJobSettings
{
    public bool EnableAccrualJob { get; init; } = true;

    public bool EnableCarryForwardJob { get; init; } = true;

    public bool EnableEscalationJob { get; init; } = true;
}
