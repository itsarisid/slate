namespace Alphabet.Domain.Enums;

/// <summary>
/// Supported second-factor delivery methods.
/// </summary>
public enum TwoFactorMethod
{
    None = 0,
    Authenticator = 1,
    Email = 2,
    Sms = 3
}
