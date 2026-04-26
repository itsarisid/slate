using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Domain.Entities;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Creates and validates access, refresh, and MFA tokens.
/// </summary>
public interface ITokenService
{
    Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<string> CreateMfaTokenAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<Guid?> GetUserIdFromMfaTokenAsync(string mfaToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? ipAddress, CancellationToken cancellationToken);

    Task RevokeAllRefreshTokensAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken);
}
