using System.Text.RegularExpressions;
using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a validated email address.
/// </summary>
public sealed partial record Email
{
    private static readonly Regex EmailRegex = EmailValidationRegex();

    public string Value { get; }

    // EF Core needs this - mark private to keep your domain clean
    private Email() { Value = string.Empty; }    // For EF Core

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
        {
            throw new DomainException("A valid email address is required.");
        }

        return new Email(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailValidationRegex();
}
