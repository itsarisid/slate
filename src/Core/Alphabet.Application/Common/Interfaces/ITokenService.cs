using Alphabet.Application.Features.Identity.Dtos;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Creates and validates access, refresh, and MFA tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Create auth response async.
    /// </summary>
    Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken);
    /// <summary>
    /// Create mfa token async.
    /// </summary>

    Task<string> CreateMfaTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
    /// <summary>
    /// Get user id from mfa token async.
    /// </summary>

    Task<Guid?> GetUserIdFromMfaTokenAsync(string mfaToken, CancellationToken cancellationToken);
    /// <summary>
    /// Get refresh token async.
    /// </summary>

    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    /// <summary>
    /// Revoke refresh token async.
    /// </summary>

    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? ipAddress, CancellationToken cancellationToken);
    /// <summary>
    /// Revoke all refresh tokens async.
    /// </summary>

    Task RevokeAllRefreshTokensAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken);
}
