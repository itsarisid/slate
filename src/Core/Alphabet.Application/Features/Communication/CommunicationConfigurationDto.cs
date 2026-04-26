namespace Alphabet.Application.Features.Communication;

/// <summary>
/// Represents the currently enabled communication configuration.
/// </summary>
public sealed record CommunicationConfigurationDto(
    IReadOnlyCollection<string> EnabledChannels,
    string DefaultChannel,
    bool DetailedLoggingEnabled);
