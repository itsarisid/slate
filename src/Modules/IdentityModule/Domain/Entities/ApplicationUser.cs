using Alphabet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an authenticated application user.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsTwoFactorEnabled { get; set; }

    public TwoFactorMethod TwoFactorMethod { get; set; }

    public string? RecoveryCodes { get; set; }

    public string? OtpDestination { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
