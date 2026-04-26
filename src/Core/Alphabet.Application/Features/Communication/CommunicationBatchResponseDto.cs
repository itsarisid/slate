namespace Alphabet.Application.Features.Communication;

/// <summary>
/// Represents a batch communication delivery response.
/// </summary>
public sealed record CommunicationBatchResponseDto(
    string Subject,
    int TotalChannelsRequested,
    int SuccessfulChannels,
    int FailedChannels,
    IReadOnlyList<CommunicationDeliveryResultDto> Results);

/// <summary>
/// Represents a single channel delivery result returned to callers.
/// </summary>
public sealed record CommunicationDeliveryResultDto(
    string Channel,
    bool IsSuccess,
    string Message,
    DateTimeOffset ProcessedAt);
