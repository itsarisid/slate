namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Provides time for application services.
/// </summary>
public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
