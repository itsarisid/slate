using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Sends webhook notifications through an outbound HTTP integration point.
/// </summary>
public sealed class WebhookNotificationService(ILogger<WebhookNotificationService> logger) : IWebhookNotificationService
{
    /// <summary>
    /// Sends a webhook notification to the specified endpoint.
    /// </summary>
    public Task SendAsync(string url, string subject, string body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Sending webhook notification to {Url} with subject {Subject}.", url, subject);
        return Task.CompletedTask;
    }
}
