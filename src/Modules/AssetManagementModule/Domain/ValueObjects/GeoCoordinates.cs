using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents geo coordinates for a location.
/// </summary>
[NotMapped]
public sealed record GeoCoordinates(decimal Latitude, decimal Longitude);
