namespace Alphabet.Infrastructure.Options;

/// <summary>
/// JWT authentication settings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Alphabet";

    public string Audience { get; init; } = "Alphabet.Clients";

    public string SecretKey { get; init; } = "ChangeThisSecretKeyToASecureValue123!";

    public int AccessTokenExpiryMinutes { get; init; } = 15;

    public int RefreshTokenExpiryDays { get; init; } = 7;

    public int MfaTokenExpiryMinutes { get; init; } = 5;

    public string SigningAlgorithm { get; init; } = "HS256";
}
