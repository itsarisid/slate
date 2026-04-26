using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Communication.Queries.GetCommunicationConfiguration;

/// <summary>
/// Handles communication configuration queries.
/// </summary>
public sealed class GetCommunicationConfigurationQueryHandler(ICommunicationService communicationService)
    : IRequestHandler<GetCommunicationConfigurationQuery, Result<CommunicationConfigurationDto>>
{
    /// <summary>
    /// Returns the effective communication configuration.
    /// </summary>
    public Task<Result<CommunicationConfigurationDto>> Handle(
        GetCommunicationConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configuration = communicationService.GetConfiguration();
        return Task.FromResult(
            Result<CommunicationConfigurationDto>.Success(
                new CommunicationConfigurationDto(
                    configuration.EnabledChannels,
                    configuration.DefaultChannel,
                    configuration.DetailedLoggingEnabled)));
    }
}
