using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Provides authentication and identity workflows for application handlers.
/// </summary>
public interface IIdentityService
{
    Task<Result<UserDto>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<AuthResponseDto>> CompleteMfaLoginAsync(string mfaToken, string verificationCode, CancellationToken cancellationToken);

    Task<Result> SendForgotPasswordAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    Task<Result<AuthenticatorSetupDto>> EnableAuthenticatorAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<RecoveryCodesDto>> VerifyAuthenticatorAsync(Guid userId, string verificationCode, CancellationToken cancellationToken);

    Task<Result> EnableOtpAsync(Guid userId, TwoFactorMethod method, string destination, CancellationToken cancellationToken);

    Task<Result> VerifyOtpAsync(Guid userId, string destination, string verificationCode, CancellationToken cancellationToken);

    Task<Result<RecoveryCodesDto>> GetRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<RecoveryCodesDto>> RegenerateRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result> UnlockUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken);
}
