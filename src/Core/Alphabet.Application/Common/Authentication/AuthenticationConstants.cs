namespace Alphabet.Application.Common.Authentication;

/// <summary>
/// Defines authentication-related constants shared across the solution.
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// The bearer authentication scheme name.
    /// </summary>
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// The configuration section containing cookie authentication settings.
    /// </summary>
    public const string CookieSettingsSection = "Authentication:Cookies";

    /// <summary>
    /// The default access token cookie name.
    /// </summary>
    public const string DefaultAccessTokenCookieName = "alphabet_access_token";

    /// <summary>
    /// The default refresh token cookie name.
    /// </summary>
    public const string DefaultRefreshTokenCookieName = "alphabet_refresh_token";
}
