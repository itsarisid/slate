namespace Alphabet.Infrastructure.Options;

/// <summary>
/// SMS provider settings.
/// </summary>
public sealed class SmsSettings
{
    public const string SectionName = "SmsSettings";

    public string AccountSid { get; init; } = string.Empty;

    public string AuthToken { get; init; } = string.Empty;

    public string FromNumber { get; init; } = string.Empty;
}
