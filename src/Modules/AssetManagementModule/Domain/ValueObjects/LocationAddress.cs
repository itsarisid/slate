using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a location address.
/// </summary>
[NotMapped]
public sealed record LocationAddress(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);
