namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configures productivity module behavior.
/// </summary>
public sealed class ProductivitySettings
{
    public const string SectionName = "ProductivitySettings";

    public int ReminderCheckIntervalSeconds { get; init; } = 60;

    public int DefaultReminderMinutesBefore { get; init; } = 15;

    public int MaxRecurringEvents { get; init; } = 100;

    public int TrashRetentionDays { get; init; } = 30;

    public int MaxNoteVersions { get; init; } = 50;

    public int MaxAttachmentSizeMB { get; init; } = 10;

    public string[] AllowedAttachmentTypes { get; init; } = [".jpg", ".png", ".pdf", ".docx", ".txt"];

    public bool EnableRealTimeSync { get; init; } = true;

    public bool FullTextSearchEnabled { get; init; } = true;

    public string DefaultCalendarView { get; init; } = "Month";
}
