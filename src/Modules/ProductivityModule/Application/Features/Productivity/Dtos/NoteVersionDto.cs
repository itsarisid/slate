namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a note version entry.
/// </summary>
public sealed record NoteVersionDto(
    int VersionNumber,
    string Content,
    DateTimeOffset SavedAt);
