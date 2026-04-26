using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Communication.Commands.SendCommunication;

/// <summary>
/// Handles communication dispatch requests.
/// </summary>
public sealed class SendCommunicationCommandHandler(ICommunicationService communicationService)
    : IRequestHandler<SendCommunicationCommand, Result<CommunicationBatchResponseDto>>
{
    /// <summary>
    /// Sends the requested message across the requested channels.
    /// </summary>
    public async Task<Result<CommunicationBatchResponseDto>> Handle(
        SendCommunicationCommand request,
        CancellationToken cancellationToken)
    {
        var dispatchRequest = new CommunicationDispatchRequest(
            request.Subject,
            request.Body,
            request.Channels,
            request.EmailAddress,
            request.PhoneNumber,
            request.UserId,
            request.PushToken,
            request.WebhookUrl,
            request.IsHtml);

        var deliveryResults = await communicationService.SendAsync(dispatchRequest, cancellationToken);
        var response = new CommunicationBatchResponseDto(
            request.Subject,
            deliveryResults.Count,
            deliveryResults.Count(result => result.IsSuccess),
            deliveryResults.Count(result => !result.IsSuccess),
            deliveryResults
                .Select(result => new CommunicationDeliveryResultDto(
                    result.Channel,
                    result.IsSuccess,
                    result.Message,
                    result.ProcessedAt))
                .ToArray());

        return Result<CommunicationBatchResponseDto>.Success(response);
    }
}
