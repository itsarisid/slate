namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents authentication metadata for a user.
/// </summary>
public sealed class Auth : BaseEntity
{
    private Auth()
    {
    }

    public Guid UserId { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public string? ResetToken { get; private set; }

    public DateTimeOffset? ResetTokenExpiresAt { get; private set; }

    public static Auth Create(Guid userId, string passwordHash)
    {
        return new Auth
        {
            UserId = userId,
            PasswordHash = passwordHash
        };
    }

    public void SetResetToken(string token, DateTimeOffset expiresAt)
    {
        ResetToken = token;
        ResetTokenExpiresAt = expiresAt;
        Touch();
    }
}
