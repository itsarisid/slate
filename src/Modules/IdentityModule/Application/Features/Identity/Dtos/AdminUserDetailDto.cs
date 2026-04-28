namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Detailed user information returned for admin views.
/// </summary>
public sealed record AdminUserDetailDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    bool IsTwoFactorEnabled,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<string> Roles);
