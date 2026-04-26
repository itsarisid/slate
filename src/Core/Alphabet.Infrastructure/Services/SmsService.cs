using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Sends SMS messages through an external provider integration point.
/// </summary>
public sealed class SmsService(ILogger<SmsService> logger) : ISmsService
{
    public Task SendAsync(string to, string body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Sending SMS to {To}.", to);
        return Task.CompletedTask;
    }
}
