namespace Alphabet.Modules.IdentityModule.Api.Models;

/// <summary>
/// Request model for admin creating a new user.
/// </summary>
public sealed record AdminCreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role = "User");

/// <summary>
/// Request model for admin locking a user account.
/// Pass 0 for indefinite lockout.
/// </summary>
public sealed record AdminLockUserRequest(int DurationMinutes = 0);

/// <summary>
/// Request model for admin resetting a user's password.
/// </summary>
public sealed record AdminResetPasswordRequest(string NewPassword);
