using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Communication.Queries.GetCommunicationConfiguration;

/// <summary>
/// Gets the effective communication module configuration.
/// </summary>
public sealed record GetCommunicationConfigurationQuery : IRequest<Result<CommunicationConfigurationDto>>;
