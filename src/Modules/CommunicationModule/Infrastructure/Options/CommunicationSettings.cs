namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Communication module settings.
/// </summary>
public sealed class CommunicationSettings
{
    public const string SectionName = "Communication";

    public string[] EnabledChannels { get; init; } = ["Email", "Sms", "Push", "InApp"];

    public string DefaultChannel { get; init; } = "Email";

    public bool EnableDetailedLogging { get; init; } = true;
}
