namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents the response payload for authentication operations.
/// </summary>
public sealed record AuthResponseDto(
    string? AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string TokenType,
    bool RequiresTwoFactor,
    string? MfaToken,
    string? Message);
