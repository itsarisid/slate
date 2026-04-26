using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Provides MFA setup and verification services.
/// </summary>
public sealed class TwoFactorService(
    UserManager<ApplicationUser> userManager,
    IDistributedCache distributedCache,
    IEmailService emailService,
    ISmsService smsService,
    IOptions<MfaSettings> mfaOptions)
    : ITwoFactorService
{
    private readonly MfaSettings _settings = mfaOptions.Value;

    public async Task<(string SharedKey, string AuthenticatorUri, string ManualEntryKey)> GenerateAuthenticatorSetupAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        key ??= string.Empty;
        var issuer = Uri.EscapeDataString("Alphabet");
        var account = Uri.EscapeDataString(user.Email ?? user.UserName ?? user.Id.ToString());
        var uri = $"otpauth://totp/{issuer}:{account}?secret={key}&issuer={issuer}&digits={_settings.AuthenticatorCodeLength}";
        return (key, uri, key);
    }

    public Task<bool> VerifyAuthenticatorCodeAsync(ApplicationUser user, string verificationCode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = verificationCode.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
        return userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, normalized);
    }

    public async Task<string> SendOtpAsync(
        ApplicationUser user,
        TwoFactorMethod method,
        string destination,
        CancellationToken cancellationToken)
    {
        var code = GenerateNumericCode(_settings.OtpCodeLength);
        var cacheKey = $"mfa:otp:{user.Id}:{destination}";
        var payload = JsonSerializer.Serialize(new OtpCacheItem(code, method));

        await distributedCache.SetStringAsync(
            cacheKey,
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.OtpExpiryMinutes)
            },
            cancellationToken);

        if (method == TwoFactorMethod.Email)
        {
            await emailService.SendAsync(destination, "Your OTP code", EmailTemplates.Otp(code), cancellationToken);
        }
        else
        {
            await smsService.SendAsync(destination, EmailTemplates.Otp(code), cancellationToken);
        }

        return code;
    }

    public async Task<bool> VerifyOtpAsync(ApplicationUser user, string destination, string verificationCode, CancellationToken cancellationToken)
    {
        var cacheKey = $"mfa:otp:{user.Id}:{destination}";
        var payload = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var item = JsonSerializer.Deserialize<OtpCacheItem>(payload);
        var valid = item is not null && string.Equals(item.Code, verificationCode, StringComparison.Ordinal);
        if (valid)
        {
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }

        return valid;
    }

    public IReadOnlyCollection<string> GenerateRecoveryCodes(int count)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            codes.Add(Convert.ToHexString(RandomNumberGenerator.GetBytes(5)));
        }

        return codes;
    }

    public bool VerifyRecoveryCode(ApplicationUser user, string verificationCode, out IReadOnlyCollection<string> remainingCodes)
    {
        var entries = ParseRecoveryCodes(user.RecoveryCodes);
        var matched = entries.Remove(Hash(verificationCode));
        remainingCodes = entries;
        return matched;
    }

    internal static string SerializeRecoveryCodes(IReadOnlyCollection<string> codes)
        => JsonSerializer.Serialize(codes.Select(Hash));

    internal static List<string> ParseRecoveryCodes(string? serialized)
        => string.IsNullOrWhiteSpace(serialized)
            ? []
            : JsonSerializer.Deserialize<List<string>>(serialized) ?? [];

    private static string GenerateNumericCode(int length)
    {
        const string chars = "0123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var builder = new StringBuilder(length);
        foreach (var value in bytes)
        {
            builder.Append(chars[value % chars.Length]);
        }

        return builder.ToString();
    }

    private static string Hash(string value)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record OtpCacheItem(string Code, TwoFactorMethod Method);
}
