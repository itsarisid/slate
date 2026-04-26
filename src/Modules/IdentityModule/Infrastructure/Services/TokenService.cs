using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Generates and validates access, refresh, and MFA tokens.
/// </summary>
public sealed class TokenService(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> options)
    : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var accessToken = await CreateJwtTokenAsync(
            user,
            DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            null);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
            CreatedByIp = currentUserService.IpAddress
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            refreshToken.Token,
            _settings.AccessTokenExpiryMinutes * 60,
            "Bearer",
            false,
            null,
            "Authentication successful.");
    }

    public Task<string> CreateMfaTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return CreateJwtTokenAsync(
            user,
            DateTimeOffset.UtcNow.AddMinutes(_settings.MfaTokenExpiryMinutes),
            [new Claim("token_use", "mfa")]);
    }

    public Task<Guid?> GetUserIdFromMfaTokenAsync(string mfaToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var principal = ValidateToken(mfaToken, true);
        var tokenUse = principal?.FindFirstValue("token_use");

        return Task.FromResult(
            tokenUse == "mfa" && Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : (Guid?)null);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        => dbContext.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken)
    {
        var tokens = await dbContext.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> CreateJwtTokenAsync(
        ApplicationUser user,
        DateTimeOffset expiresAt,
        IEnumerable<Claim>? additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(additionalClaims ?? []);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
    {
        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = validateLifetime,
                    ValidIssuer = _settings.Issuer,
                    ValidAudience = _settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                },
                out _);
        }
        catch
        {
            return null;
        }
    }
}
