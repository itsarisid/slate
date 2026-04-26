using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Communication.Commands.SendCommunication;

/// <summary>
/// Sends a notification or message through one or more configured channels.
/// </summary>
public sealed record SendCommunicationCommand(
    string Subject,
    string Body,
    IReadOnlyCollection<string> Channels,
    string? EmailAddress,
    string? PhoneNumber,
    string? UserId,
    string? PushToken,
    string? WebhookUrl,
    bool IsHtml) : IRequest<Result<CommunicationBatchResponseDto>>;
