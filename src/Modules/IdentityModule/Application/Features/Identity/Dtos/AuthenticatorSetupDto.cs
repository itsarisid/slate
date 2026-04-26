namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents authenticator application setup data.
/// </summary>
public sealed record AuthenticatorSetupDto(string SharedKey, string AuthenticatorUri, string ManualEntryKey);
