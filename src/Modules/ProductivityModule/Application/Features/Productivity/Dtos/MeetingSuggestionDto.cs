namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents an AI-assisted meeting suggestion.
/// </summary>
public sealed record MeetingSuggestionDto(
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal Score);
