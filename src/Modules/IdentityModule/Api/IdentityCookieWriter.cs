using Alphabet.Application.Common.Authentication;
using Alphabet.Application.Features.Identity.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Modules.IdentityModule.Api;
/// <summary>
/// Identity cookie writer.
/// </summary>

internal static class IdentityCookieWriter
{
    /// <summary>
    /// Write auth cookies.
    /// </summary>
    public static void WriteAuthCookies(HttpContext context, AuthResponseDto response)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken) || string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            return;
        }

        var cookieSettings = ReadCookieSettings(context);
        var sameSite = ParseSameSite(cookieSettings["SameSite"]);
        var secure = bool.TryParse(cookieSettings["Secure"], out var secureValue) ? secureValue : true;
        var httpOnly = !bool.TryParse(cookieSettings["HttpOnly"], out var httpOnlyValue) || httpOnlyValue;
        var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? AuthenticationConstants.DefaultAccessTokenCookieName;
        var refreshTokenCookieName = cookieSettings["RefreshTokenCookieName"] ?? AuthenticationConstants.DefaultRefreshTokenCookieName;

        context.Response.Cookies.Append(
            accessTokenCookieName,
            response.AccessToken,
            new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = secure,
                SameSite = sameSite,
                Expires = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn),
                IsEssential = true
            });

        context.Response.Cookies.Append(
            refreshTokenCookieName,
            response.RefreshToken,
            new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = secure,
                SameSite = sameSite,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                IsEssential = true
            });
    }
    /// <summary>
    /// Clear auth cookies.
    /// </summary>

    public static void ClearAuthCookies(HttpContext context)
    {
        var cookieSettings = ReadCookieSettings(context);
        var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? AuthenticationConstants.DefaultAccessTokenCookieName;
        var refreshTokenCookieName = cookieSettings["RefreshTokenCookieName"] ?? AuthenticationConstants.DefaultRefreshTokenCookieName;

        context.Response.Cookies.Delete(accessTokenCookieName);
        context.Response.Cookies.Delete(refreshTokenCookieName);
    }
    /// <summary>
    /// Get refresh token from cookie.
    /// </summary>

    public static string? GetRefreshTokenFromCookie(HttpContext context)
    {
        var cookieSettings = ReadCookieSettings(context);
        var refreshTokenCookieName = cookieSettings["RefreshTokenCookieName"] ?? AuthenticationConstants.DefaultRefreshTokenCookieName;
        return context.Request.Cookies.TryGetValue(refreshTokenCookieName, out var refreshToken) ? refreshToken : null;
    }
    /// <summary>
    /// Read cookie settings.
    /// </summary>

    private static IConfigurationSection ReadCookieSettings(HttpContext context)
    {
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        return configuration.GetSection(AuthenticationConstants.CookieSettingsSection);
    }
    /// <summary>
    /// Parse same site.
    /// </summary>

    private static SameSiteMode ParseSameSite(string? sameSite)
    {
        return sameSite?.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }
}
