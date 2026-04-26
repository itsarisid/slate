using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a message body value object.
/// </summary>
public sealed record Message
{
    public string Subject { get; init; }

    public string Body { get; init; }

    public Message(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new DomainException("Message subject is required.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("Message body is required.");
        }

        Subject = subject.Trim();
        Body = body.Trim();
    }
}
