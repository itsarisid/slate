using Alphabet.Application.Common.Authentication;

namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Cookie-based authentication settings.
/// </summary>
public sealed class CookieAuthenticationSettings
{
    public const string SectionName = AuthenticationConstants.CookieSettingsSection;

    public string AccessTokenCookieName { get; init; } = AuthenticationConstants.DefaultAccessTokenCookieName;

    public string RefreshTokenCookieName { get; init; } = AuthenticationConstants.DefaultRefreshTokenCookieName;

    public bool HttpOnly { get; init; } = true;

    public bool Secure { get; init; } = true;

    public string SameSite { get; init; } = "Lax";
}
