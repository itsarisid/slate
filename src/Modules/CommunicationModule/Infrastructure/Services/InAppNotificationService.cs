using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Sends in-application notifications through the configured notification store.
/// </summary>
public sealed class InAppNotificationService(ILogger<InAppNotificationService> logger) : IInAppNotificationService
{
    /// <summary>
    /// Sends an in-application notification to the specified user.
    /// </summary>
    public Task SendAsync(string userId, string subject, string body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Sending in-app notification to user {UserId} with subject {Subject}.",
            userId,
            subject);
        return Task.CompletedTask;
    }
}
