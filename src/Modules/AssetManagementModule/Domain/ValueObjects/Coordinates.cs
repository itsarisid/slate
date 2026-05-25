namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents geographic coordinates for a location.
/// </summary>
public sealed record Coordinates(decimal Latitude, decimal Longitude);
