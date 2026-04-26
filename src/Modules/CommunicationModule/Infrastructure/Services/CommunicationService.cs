using Alphabet.Application.Common.Interfaces;
using Alphabet.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Dispatches communication requests across configured delivery channels.
/// </summary>
public sealed class CommunicationService(
    IEmailService emailService,
    ISmsService smsService,
    IPushNotificationService pushNotificationService,
    IInAppNotificationService inAppNotificationService,
    IWebhookNotificationService webhookNotificationService,
    IOptions<CommunicationSettings> communicationOptions,
    ILogger<CommunicationService> logger)
    : ICommunicationService
{
    private readonly CommunicationSettings _settings = communicationOptions.Value;

    /// <summary>
    /// Sends a message to one or more configured channels.
    /// </summary>
    public async Task<IReadOnlyList<CommunicationDispatchResult>> SendAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        var requestedChannels = NormalizeRequestedChannels(request.Channels);
        var enabledChannels = GetEnabledChannels();
        var results = new List<CommunicationDispatchResult>(requestedChannels.Count);

        foreach (var channel in requestedChannels)
        {
            if (!enabledChannels.Contains(channel))
            {
                results.Add(new CommunicationDispatchResult(
                    channel,
                    false,
                    $"Channel '{channel}' is disabled by configuration.",
                    DateTimeOffset.UtcNow));
                continue;
            }

            var result = channel switch
            {
                "Email" => await SendEmailAsync(request, cancellationToken),
                "Sms" => await SendSmsAsync(request, cancellationToken),
                "Push" => await SendPushAsync(request, cancellationToken),
                "InApp" => await SendInAppAsync(request, cancellationToken),
                "Webhook" => await SendWebhookAsync(request, cancellationToken),
                _ => new CommunicationDispatchResult(
                    channel,
                    false,
                    $"Channel '{channel}' is not supported.",
                    DateTimeOffset.UtcNow)
            };

            if (_settings.EnableDetailedLogging)
            {
                logger.LogInformation(
                    "Communication dispatch on channel {Channel} completed with success={IsSuccess}.",
                    result.Channel,
                    result.IsSuccess);
            }

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Gets the active communication configuration snapshot.
    /// </summary>
    public CommunicationConfigurationSnapshot GetConfiguration()
    {
        return new CommunicationConfigurationSnapshot(
            GetEnabledChannels().ToArray(),
            NormalizeChannel(_settings.DefaultChannel),
            _settings.EnableDetailedLogging);
    }

    private async Task<CommunicationDispatchResult> SendEmailAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EmailAddress))
        {
            return MissingAddress("Email", "EmailAddress");
        }

        if (request.IsHtml)
        {
            await emailService.SendTemplateAsync(request.EmailAddress, request.Subject, request.Body, cancellationToken);
        }
        else
        {
            await emailService.SendAsync(request.EmailAddress, request.Subject, request.Body, cancellationToken);
        }

        return Success("Email", $"Email queued for {request.EmailAddress}.");
    }

    private async Task<CommunicationDispatchResult> SendSmsAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return MissingAddress("Sms", "PhoneNumber");
        }

        await smsService.SendAsync(request.PhoneNumber, request.Body, cancellationToken);
        return Success("Sms", $"SMS queued for {request.PhoneNumber}.");
    }

    private async Task<CommunicationDispatchResult> SendPushAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PushToken))
        {
            return MissingAddress("Push", "PushToken");
        }

        await pushNotificationService.SendAsync(request.PushToken, request.Subject, request.Body, cancellationToken);
        return Success("Push", "Push notification queued.");
    }

    private async Task<CommunicationDispatchResult> SendInAppAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return MissingAddress("InApp", "UserId");
        }

        await inAppNotificationService.SendAsync(request.UserId, request.Subject, request.Body, cancellationToken);
        return Success("InApp", $"In-app notification queued for user {request.UserId}.");
    }

    private async Task<CommunicationDispatchResult> SendWebhookAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            return MissingAddress("Webhook", "WebhookUrl");
        }

        await webhookNotificationService.SendAsync(request.WebhookUrl, request.Subject, request.Body, cancellationToken);
        return Success("Webhook", $"Webhook notification queued for {request.WebhookUrl}.");
    }

    private HashSet<string> GetEnabledChannels()
    {
        return NormalizeRequestedChannels(_settings.EnabledChannels);
    }

    private static HashSet<string> NormalizeRequestedChannels(IEnumerable<string> channels)
    {
        var normalizedChannels = channels
            .Where(channel => !string.IsNullOrWhiteSpace(channel))
            .Select(NormalizeChannel)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalizedChannels.Count == 0)
        {
            normalizedChannels.Add("Email");
        }

        return normalizedChannels;
    }

    private static string NormalizeChannel(string channel)
    {
        return channel.Trim().ToLowerInvariant() switch
        {
            "email" => "Email",
            "sms" => "Sms",
            "push" => "Push",
            "inapp" => "InApp",
            "webhook" => "Webhook",
            _ => channel.Trim()
        };
    }

    private static CommunicationDispatchResult Success(string channel, string message)
    {
        return new CommunicationDispatchResult(channel, true, message, DateTimeOffset.UtcNow);
    }

    private static CommunicationDispatchResult MissingAddress(string channel, string fieldName)
    {
        return new CommunicationDispatchResult(
            channel,
            false,
            $"Channel '{channel}' requires '{fieldName}' to be provided.",
            DateTimeOffset.UtcNow);
    }
}
