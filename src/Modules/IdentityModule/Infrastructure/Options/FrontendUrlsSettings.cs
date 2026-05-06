namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Frontend URLs used in identity emails.
/// </summary>
public sealed class FrontendUrlsSettings
{
    public const string SectionName = "FrontendUrls";

    public string ConfirmEmail { get; init; } = "https://localhost:5001/confirm-email";

    public string ResetPassword { get; init; } = "https://localhost:5001/reset-password";
}
