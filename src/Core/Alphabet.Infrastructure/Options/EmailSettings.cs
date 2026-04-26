namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Email provider settings.
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string FromEmail { get; init; } = "noreply@example.com";

    public string FromName { get; init; } = "Alphabet";

    public string SmtpServer { get; init; } = "localhost";

    public int SmtpPort { get; init; } = 25;

    public string? ApiKey { get; init; }
}
