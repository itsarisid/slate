namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configures productivity delivery providers.
/// </summary>
public sealed class ProductivityNotificationSettings
{
    public const string SectionName = "NotificationSettings";

    public bool EmailEnabled { get; init; } = true;

    public bool SmsEnabled { get; init; }

    public bool PushEnabled { get; init; } = true;

    public string? FirebaseServerKey { get; init; }

    public string? SendGridApiKey { get; init; }
}
