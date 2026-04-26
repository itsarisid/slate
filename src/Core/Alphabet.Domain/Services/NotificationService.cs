using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Services;

/// <summary>
/// Provides simple notification domain logic.
/// </summary>
public sealed class NotificationService
{
    public Message CreateWelcomeMessage(string firstName)
    {
        return new Message("Welcome", $"Hello {firstName}, your account is ready.");
    }
}
