using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Provides authentication and identity workflows for application handlers.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Register async.
    /// </summary>
    Task<Result<UserDto>> RegisterAsync(
    string email,
    string password,
    string firstName,
    string lastName,
    CancellationToken cancellationToken);
    /// <summary>
    /// Confirm email async.
    /// </summary>

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);
    /// <summary>
    /// Login async.
    /// </summary>

    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    /// <summary>
    /// Complete mfa login async.
    /// </summary>

    Task<Result<AuthResponseDto>> CompleteMfaLoginAsync(string mfaToken, string verificationCode, CancellationToken cancellationToken);
    /// <summary>
    /// Send forgot password async.
    /// </summary>

    Task<Result> SendForgotPasswordAsync(string email, CancellationToken cancellationToken);
    /// <summary>
    /// Reset password async.
    /// </summary>

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    /// <summary>
    /// Refresh token async.
    /// </summary>

    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    /// <summary>
    /// Logout async.
    /// </summary>

    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    /// <summary>
    /// Change password async.
    /// </summary>

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    /// <summary>
    /// Enable authenticator async.
    /// </summary>

    Task<Result<AuthenticatorSetupDto>> EnableAuthenticatorAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Verify authenticator async.
    /// </summary>

    Task<Result<RecoveryCodesDto>> VerifyAuthenticatorAsync(Guid userId, string verificationCode, CancellationToken cancellationToken);
    /// <summary>
    /// Enable otp async.
    /// </summary>

    Task<Result> EnableOtpAsync(Guid userId, TwoFactorMethod method, string destination, CancellationToken cancellationToken);
    /// <summary>
    /// Verify otp async.
    /// </summary>

    Task<Result> VerifyOtpAsync(Guid userId, string destination, string verificationCode, CancellationToken cancellationToken);
    /// <summary>
    /// Get recovery codes async.
    /// </summary>

    Task<Result<RecoveryCodesDto>> GetRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Regenerate recovery codes async.
    /// </summary>

    Task<Result<RecoveryCodesDto>> RegenerateRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Unlock user async.
    /// </summary>

    Task<Result> UnlockUserAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Get users async.
    /// </summary>

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken);

    // ── Admin management ──────────────────────────────────────────────

    /// <summary>
    /// Admin creates a new user with optional role assignment and auto-confirmed email.
    /// </summary>
    Task<Result<UserDto>> AdminCreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken);

    /// <summary>
    /// Admin locks a user account for a given duration.
    /// </summary>
    Task<Result> LockUserAsync(Guid userId, int durationMinutes, CancellationToken cancellationToken);

    /// <summary>
    /// Admin resets a user's password without requiring the old password.
    /// </summary>
    Task<Result> AdminResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Admin sends a password-reset link to the user's email.
    /// </summary>
    Task<Result> AdminSendResetLinkAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Admin force-logs out a user by revoking all refresh tokens.
    /// </summary>
    Task<Result> AdminForceLogoutAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns detailed user information for admin views.
    /// </summary>
    Task<Result<AdminUserDetailDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns audit log entries for a given user.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetUserAuditLogsAsync(Guid userId, int take, int skip, CancellationToken cancellationToken);
}
