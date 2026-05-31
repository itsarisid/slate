namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a postal address.
/// </summary>
public sealed record Address(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);
