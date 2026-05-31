using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a scheduled reminder.
/// </summary>
public sealed class Reminder : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset ReminderTime { get; private set; }

    public ReminderType ReminderType { get; private set; }

    public int? RepeatInterval { get; private set; }

    public int? RepeatCount { get; private set; }

    public DateTimeOffset? EndDate { get; private set; }

    public bool SoundEnabled { get; private set; }

    public bool VibrationEnabled { get; private set; }

    public bool SnoozeEnabled { get; private set; }

    public int? SnoozeMinutes { get; private set; }

    public string? LinkedEntityType { get; private set; }

    public Guid? LinkedEntityId { get; private set; }

    public string NotificationMethodsJson { get; private set; } = "[]";

    public ReminderStatus Status { get; private set; } = ReminderStatus.Active;

    public string? RecurrencePatternJson { get; private set; }

    private Reminder()
    {
    }

    private Reminder(
        Guid ownerUserId,
        string title,
        string description,
        DateTimeOffset reminderTime,
        ReminderType reminderType,
        int? repeatInterval,
        int? repeatCount,
        DateTimeOffset? endDate,
        bool soundEnabled,
        bool vibrationEnabled,
        bool snoozeEnabled,
        int? snoozeMinutes,
        string? linkedEntityType,
        Guid? linkedEntityId,
        IReadOnlyCollection<string> notificationMethods,
        RecurrencePattern? recurrencePattern)
    {
        OwnerUserId = ownerUserId;
        Title = title.Trim();
        Description = description.Trim();
        ReminderTime = reminderTime.ToUniversalTime();
        ReminderType = reminderType;
        RepeatInterval = repeatInterval;
        RepeatCount = repeatCount;
        EndDate = endDate?.ToUniversalTime();
        SoundEnabled = soundEnabled;
        VibrationEnabled = vibrationEnabled;
        SnoozeEnabled = snoozeEnabled;
        SnoozeMinutes = snoozeMinutes;
        LinkedEntityType = string.IsNullOrWhiteSpace(linkedEntityType) ? null : linkedEntityType.Trim();
        LinkedEntityId = linkedEntityId;
        NotificationMethodsJson = ProductivityJson.Serialize(notificationMethods);
        RecurrencePatternJson = recurrencePattern is null ? null : ProductivityJson.Serialize(recurrencePattern);
    }
    /// <summary>
    /// Create.
    /// </summary>

    public static Reminder Create(
        Guid ownerUserId,
        string title,
        string description,
        DateTimeOffset reminderTime,
        ReminderType reminderType,
        int? repeatInterval,
        int? repeatCount,
        DateTimeOffset? endDate,
        bool soundEnabled,
        bool vibrationEnabled,
        bool snoozeEnabled,
        int? snoozeMinutes,
        string? linkedEntityType,
        Guid? linkedEntityId,
        IReadOnlyCollection<string> notificationMethods,
        RecurrencePattern? recurrencePattern)
        => new(
            ownerUserId,
            title,
            description,
            reminderTime,
            reminderType,
            repeatInterval,
            repeatCount,
            endDate,
            soundEnabled,
            vibrationEnabled,
            snoozeEnabled,
            snoozeMinutes,
            linkedEntityType,
            linkedEntityId,
            notificationMethods,
            recurrencePattern);

    public IReadOnlyList<string> NotificationMethods => ProductivityJson.DeserializeList<string>(NotificationMethodsJson);
    /// <summary>
    /// Snooze.
    /// </summary>

    public void Snooze(int minutes)
    {
        ReminderTime = ReminderTime.AddMinutes(minutes);
        Status = ReminderStatus.Snoozed;
        Touch();
    }
    /// <summary>
    /// Dismiss.
    /// </summary>

    public void Dismiss()
    {
        Status = ReminderStatus.Dismissed;
        Touch();
    }
    /// <summary>
    /// Mark triggered.
    /// </summary>

    public void MarkTriggered()
    {
        Status = ReminderStatus.Triggered;
        Touch();
    }
    /// <summary>
    /// Mark completed.
    /// </summary>

    public void MarkCompleted()
    {
        Status = ReminderStatus.Completed;
        Touch();
    }
}
