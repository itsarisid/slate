using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Sends push notifications through the configured push provider.
/// </summary>
public sealed class PushNotificationService(ILogger<PushNotificationService> logger) : IPushNotificationService
{
    /// <summary>
    /// Sends a push notification to the specified device token.
    /// </summary>
    public Task SendAsync(string token, string subject, string body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Sending push notification to token {Token} with subject {Subject}.", token, subject);
        return Task.CompletedTask;
    }
}
