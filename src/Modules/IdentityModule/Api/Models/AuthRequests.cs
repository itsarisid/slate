namespace Alphabet.Modules.IdentityModule.Api.Models;

/// <summary>
/// Represents a login request and optional cookie-based sign-in preference.
/// </summary>
public sealed record LoginRequest(string Email, string Password, bool UseCookies);

/// <summary>
/// Represents a refresh-token request and optional cookie-based sign-in preference.
/// </summary>
public sealed record RefreshTokenRequest(string? RefreshToken, bool UseCookies);

/// <summary>
/// Represents an MFA login request and optional cookie-based sign-in preference.
/// </summary>
public sealed record MfaLoginRequest(string MfaToken, string VerificationCode, bool UseCookies);

/// <summary>
/// Represents a logout request.
/// </summary>
public sealed record LogoutRequest(string? RefreshToken);
