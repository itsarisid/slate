namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Multi-factor authentication settings.
/// </summary>
public sealed class MfaSettings
{
    public const string SectionName = "MfaSettings";

    public int AuthenticatorCodeLength { get; init; } = 6;

    public int OtpCodeLength { get; init; } = 6;

    public int OtpExpiryMinutes { get; init; } = 5;

    public int RecoveryCodeCount { get; init; } = 10;
}
