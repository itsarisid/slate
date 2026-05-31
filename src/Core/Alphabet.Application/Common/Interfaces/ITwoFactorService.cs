using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Encapsulates MFA setup and verification logic.
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Generate authenticator setup async.
    /// </summary>
    Task<(string SharedKey, string AuthenticatorUri, string ManualEntryKey)> GenerateAuthenticatorSetupAsync(
    ApplicationUser user,
    CancellationToken cancellationToken);
    /// <summary>
    /// Verify authenticator code async.
    /// </summary>

    Task<bool> VerifyAuthenticatorCodeAsync(ApplicationUser user, string verificationCode, CancellationToken cancellationToken);
    /// <summary>
    /// Send otp async.
    /// </summary>

    Task<string> SendOtpAsync(
        ApplicationUser user,
        TwoFactorMethod method,
        string destination,
        CancellationToken cancellationToken);
    /// <summary>
    /// Verify otp async.
    /// </summary>

    Task<bool> VerifyOtpAsync(
        ApplicationUser user,
        string destination,
        string verificationCode,
        CancellationToken cancellationToken);
    /// <summary>
    /// Generate recovery codes.
    /// </summary>

    IReadOnlyCollection<string> GenerateRecoveryCodes(int count);
    /// <summary>
    /// Verify recovery code.
    /// </summary>

    bool VerifyRecoveryCode(ApplicationUser user, string verificationCode, out IReadOnlyCollection<string> remainingCodes);
}
