using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents an immutable money value object.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);
}
