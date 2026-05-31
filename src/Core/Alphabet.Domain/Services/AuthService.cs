using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Services;

/// <summary>
/// Provides domain-level authentication logic.
/// </summary>
public sealed class AuthService
{
    /// <summary>
    /// Can reset password.
    /// </summary>
    public bool CanResetPassword(Auth auth, DateTimeOffset now)
    {
        return auth.ResetTokenExpiresAt is null || auth.ResetTokenExpiresAt <= now;
    }
}
