using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents an attendee response to a calendar invitation.
/// </summary>
[NotMapped]
public sealed record CalendarEventResponse(string Email, string Response, DateTimeOffset RespondedAt);
