using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Encapsulates MFA setup and verification logic.
/// </summary>
public interface ITwoFactorService
{
    Task<(string SharedKey, string AuthenticatorUri, string ManualEntryKey)> GenerateAuthenticatorSetupAsync(
        ApplicationUser user,
        CancellationToken cancellationToken);

    Task<bool> VerifyAuthenticatorCodeAsync(ApplicationUser user, string verificationCode, CancellationToken cancellationToken);

    Task<string> SendOtpAsync(
        ApplicationUser user,
        TwoFactorMethod method,
        string destination,
        CancellationToken cancellationToken);

    Task<bool> VerifyOtpAsync(
        ApplicationUser user,
        string destination,
        string verificationCode,
        CancellationToken cancellationToken);

    IReadOnlyCollection<string> GenerateRecoveryCodes(int count);

    bool VerifyRecoveryCode(ApplicationUser user, string verificationCode, out IReadOnlyCollection<string> remainingCodes);
}
