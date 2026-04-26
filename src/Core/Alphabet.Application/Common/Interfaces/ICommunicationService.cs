namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Dispatches messages across one or more configured communication channels.
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Sends a message across the requested channels.
    /// </summary>
    Task<IReadOnlyList<CommunicationDispatchResult>> SendAsync(
        CommunicationDispatchRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the currently enabled communication channels and defaults.
    /// </summary>
    CommunicationConfigurationSnapshot GetConfiguration();
}

/// <summary>
/// Represents a channel-agnostic communication dispatch request.
/// </summary>
public sealed record CommunicationDispatchRequest(
    string Subject,
    string Body,
    IReadOnlyCollection<string> Channels,
    string? EmailAddress,
    string? PhoneNumber,
    string? UserId,
    string? PushToken,
    string? WebhookUrl,
    bool IsHtml);

/// <summary>
/// Represents the result of sending to a single channel.
/// </summary>
public sealed record CommunicationDispatchResult(
    string Channel,
    bool IsSuccess,
    string Message,
    DateTimeOffset ProcessedAt);

/// <summary>
/// Represents the active communication configuration.
/// </summary>
public sealed record CommunicationConfigurationSnapshot(
    IReadOnlyCollection<string> EnabledChannels,
    string DefaultChannel,
    bool DetailedLoggingEnabled);
