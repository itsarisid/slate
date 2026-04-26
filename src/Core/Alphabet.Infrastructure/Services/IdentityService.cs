using System.Text.Encodings.Web;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Coordinates ASP.NET Core Identity workflows for the application layer.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    ITwoFactorService twoFactorService,
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IEmailService emailService,
    ICurrentUserService currentUserService,
    AppDbContext dbContext,
    IOptions<FrontendUrlsSettings> frontendUrlsOptions,
    IOptions<MfaSettings> mfaOptions)
    : IIdentityService
{
    private readonly FrontendUrlsSettings _frontendUrls = frontendUrlsOptions.Value;
    private readonly MfaSettings _mfaSettings = mfaOptions.Value;

    public async Task<Result<UserDto>> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result<UserDto>.Failure("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.MaxValue
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return Result<UserDto>.Failure(string.Join("; ", created.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, "User");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = UrlEncoder.Default.Encode(token);
        var confirmationUrl = $"{_frontendUrls.ConfirmEmail}?userId={user.Id}&token={encodedToken}";

        await emailService.SendTemplateAsync(user.Email!, "Confirm your email", EmailTemplates.Confirmation(firstName, confirmationUrl), cancellationToken);
        await emailService.SendTemplateAsync(user.Email!, "Welcome to Alphabet", EmailTemplates.Welcome(firstName), cancellationToken);
        await WriteAuditAsync(user.Id, "Register", true, "User registered.", cancellationToken);

        return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.EmailConfirmed);
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return Result.Failure(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        user.LockoutEnd = null;
        await userManager.UpdateAsync(user);
        await WriteAuditAsync(user.Id, "ConfirmEmail", true, "Email confirmed.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await WriteAuditAsync(null, "Login", false, "Invalid email.", cancellationToken);
            return Result<AuthResponseDto>.Failure("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            return Result<AuthResponseDto>.Failure("Email confirmation is required.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            await WriteAuditAsync(user.Id, "Login", false, "Account is locked.", cancellationToken);
            return Result<AuthResponseDto>.Failure("Account is currently locked.");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, true);
        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                await emailService.SendTemplateAsync(user.Email!, "Account locked", EmailTemplates.Lockout(user.FirstName, user.LockoutEnd ?? DateTimeOffset.UtcNow), cancellationToken);
                await WriteAuditAsync(user.Id, "Lockout", false, "User locked out after failed attempts.", cancellationToken);
            }

            await WriteAuditAsync(user.Id, "Login", false, "Invalid password.", cancellationToken);
            return Result<AuthResponseDto>.Failure("Invalid credentials.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        if (user.IsTwoFactorEnabled && user.TwoFactorMethod != TwoFactorMethod.None)
        {
            var mfaToken = await tokenService.CreateMfaTokenAsync(user, cancellationToken);
            await WriteAuditAsync(user.Id, "Login", true, "Password verified. MFA required.", cancellationToken);
            return new AuthResponseDto(null, null, 0, "Bearer", true, mfaToken, "Two-factor authentication required.");
        }

        var auth = await tokenService.CreateAuthResponseAsync(user, cancellationToken);
        await WriteAuditAsync(user.Id, "Login", true, "Login successful.", cancellationToken);
        return auth;
    }

    public async Task<Result<AuthResponseDto>> CompleteMfaLoginAsync(string mfaToken, string verificationCode, CancellationToken cancellationToken)
    {
        var userId = await tokenService.GetUserIdFromMfaTokenAsync(mfaToken, cancellationToken);
        if (userId is null)
        {
            return Result<AuthResponseDto>.Failure("Invalid MFA token.");
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure("User was not found.");
        }

        var valid = user.TwoFactorMethod switch
        {
            TwoFactorMethod.Authenticator => await twoFactorService.VerifyAuthenticatorCodeAsync(user, verificationCode, cancellationToken),
            TwoFactorMethod.Email or TwoFactorMethod.Sms when !string.IsNullOrWhiteSpace(user.OtpDestination)
                => await twoFactorService.VerifyOtpAsync(user, user.OtpDestination!, verificationCode, cancellationToken),
            _ => false
        };

        if (!valid)
        {
            if (twoFactorService.VerifyRecoveryCode(user, verificationCode, out var remaining))
            {
                user.RecoveryCodes = TwoFactorService.SerializeRecoveryCodes(remaining);
                await userManager.UpdateAsync(user);
            }
            else
            {
                await WriteAuditAsync(user.Id, "MfaLogin", false, "Invalid MFA code.", cancellationToken);
                return Result<AuthResponseDto>.Failure("Invalid MFA verification code.");
            }
        }

        var auth = await tokenService.CreateAuthResponseAsync(user, cancellationToken);
        await WriteAuditAsync(user.Id, "MfaLogin", true, "MFA login successful.", cancellationToken);
        return auth;
    }

    public async Task<Result> SendForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && user.EmailConfirmed)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"{_frontendUrls.ResetPassword}?email={UrlEncoder.Default.Encode(email)}&token={UrlEncoder.Default.Encode(token)}";
            await emailService.SendTemplateAsync(user.Email!, "Reset your password", EmailTemplates.PasswordReset(user.FirstName, resetUrl), cancellationToken);
            await WriteAuditAsync(user.Id, "ForgotPassword", true, "Password reset requested.", cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure("Invalid reset request.");
        }

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await tokenService.RevokeAllRefreshTokensAsync(user.Id, currentUserService.IpAddress, cancellationToken);
        await WriteAuditAsync(user.Id, "ResetPassword", true, "Password reset completed.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await tokenService.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (token is null || token.IsRevoked || token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Result<AuthResponseDto>.Failure("Refresh token is invalid.");
        }

        await tokenService.RevokeRefreshTokenAsync(token, currentUserService.IpAddress, cancellationToken);
        var auth = await tokenService.CreateAuthResponseAsync(token.User, cancellationToken);
        await WriteAuditAsync(token.UserId, "RefreshToken", true, "Refresh token rotated.", cancellationToken);
        return auth;
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await tokenService.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (token is null)
        {
            return Result.Success();
        }

        await tokenService.RevokeRefreshTokenAsync(token, currentUserService.IpAddress, cancellationToken);
        await WriteAuditAsync(token.UserId, "Logout", true, "Refresh token revoked.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await tokenService.RevokeAllRefreshTokensAsync(user.Id, currentUserService.IpAddress, cancellationToken);
        await WriteAuditAsync(user.Id, "ChangePassword", true, "Password changed.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AuthenticatorSetupDto>> EnableAuthenticatorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<AuthenticatorSetupDto>.Failure("User was not found.");
        }

        var setup = await twoFactorService.GenerateAuthenticatorSetupAsync(user, cancellationToken);
        return new AuthenticatorSetupDto(setup.SharedKey, setup.AuthenticatorUri, setup.ManualEntryKey);
    }

    public async Task<Result<RecoveryCodesDto>> VerifyAuthenticatorAsync(Guid userId, string verificationCode, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<RecoveryCodesDto>.Failure("User was not found.");
        }

        var valid = await twoFactorService.VerifyAuthenticatorCodeAsync(user, verificationCode, cancellationToken);
        if (!valid)
        {
            return Result<RecoveryCodesDto>.Failure("Authenticator verification failed.");
        }

        user.IsTwoFactorEnabled = true;
        user.TwoFactorMethod = TwoFactorMethod.Authenticator;
        var recoveryCodes = twoFactorService.GenerateRecoveryCodes(_mfaSettings.RecoveryCodeCount);
        user.RecoveryCodes = TwoFactorService.SerializeRecoveryCodes(recoveryCodes);
        await userManager.UpdateAsync(user);
        await WriteAuditAsync(user.Id, "EnableAuthenticator", true, "Authenticator MFA enabled.", cancellationToken);
        return new RecoveryCodesDto(recoveryCodes);
    }

    public async Task<Result> EnableOtpAsync(Guid userId, TwoFactorMethod method, string destination, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        await twoFactorService.SendOtpAsync(user, method, destination, cancellationToken);
        user.OtpDestination = destination;
        user.TwoFactorMethod = method;
        await userManager.UpdateAsync(user);
        await WriteAuditAsync(user.Id, "EnableOtp", true, $"OTP challenge sent via {method}.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result> VerifyOtpAsync(Guid userId, string destination, string verificationCode, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        var valid = await twoFactorService.VerifyOtpAsync(user, destination, verificationCode, cancellationToken);
        if (!valid)
        {
            return Result.Failure("OTP verification failed.");
        }

        user.IsTwoFactorEnabled = true;
        user.OtpDestination = destination;
        var recoveryCodes = twoFactorService.GenerateRecoveryCodes(_mfaSettings.RecoveryCodeCount);
        user.RecoveryCodes = TwoFactorService.SerializeRecoveryCodes(recoveryCodes);
        await userManager.UpdateAsync(user);
        await WriteAuditAsync(user.Id, "VerifyOtp", true, "OTP MFA enabled.", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<RecoveryCodesDto>> GetRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return Result<RecoveryCodesDto>.Failure("User was not found.");
        }

        var stored = TwoFactorService.ParseRecoveryCodes(user.RecoveryCodes);
        return new RecoveryCodesDto(stored);
    }

    public async Task<Result<RecoveryCodesDto>> RegenerateRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<RecoveryCodesDto>.Failure("User was not found.");
        }

        var recoveryCodes = twoFactorService.GenerateRecoveryCodes(_mfaSettings.RecoveryCodeCount);
        user.RecoveryCodes = TwoFactorService.SerializeRecoveryCodes(recoveryCodes);
        await userManager.UpdateAsync(user);
        await WriteAuditAsync(user.Id, "RegenerateRecoveryCodes", true, "Recovery codes regenerated.", cancellationToken);
        return new RecoveryCodesDto(recoveryCodes);
    }

    public async Task<Result> UnlockUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
        await WriteAuditAsync(user.Id, "UnlockUser", true, "User unlocked by admin.", cancellationToken);
        return Result.Success();
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(x => new UserDto(x.Id, x.Email ?? string.Empty, x.FirstName, x.LastName, x.EmailConfirmed)).ToArray();
    }

    private async Task WriteAuditAsync(Guid? userId, string action, bool success, string message, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                Success = success,
                Message = message,
                IpAddress = currentUserService.IpAddress,
                UserAgent = currentUserService.UserAgent
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
