namespace Alphabet.Modules.IdentityModule.Api.Models;

/// <summary>
/// Request model for admin creating a new user.
/// </summary>
public sealed record AdminCreateUserRequest
{
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the initial password that will be assigned to the user.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's given name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's family name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role assigned during user creation.
    /// </summary>
    public string Role { get; init; } = "User";
}

/// <summary>
/// Request model for admin locking a user account.
/// Pass 0 for indefinite lockout.
/// </summary>
public sealed record AdminLockUserRequest
{
    /// <summary>
    /// Gets the lock duration in minutes. Set to 0 to lock the account until it is manually unlocked.
    /// </summary>
    public int DurationMinutes { get; init; }
}

/// <summary>
/// Request model for admin resetting a user's password.
/// </summary>
public sealed record AdminResetPasswordRequest
{
    /// <summary>
    /// Gets the new password that will replace the current user password.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
}
