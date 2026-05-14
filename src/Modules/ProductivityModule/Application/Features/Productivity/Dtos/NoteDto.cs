using Alphabet.Domain.Enums;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a note read model.
/// </summary>
public sealed record NoteDto(
    Guid Id,
    string Title,
    string Content,
    NoteFormat Format,
    string? Category,
    string? Color,
    bool IsPinned,
    bool IsFavorite,
    Guid? NotebookId,
    IReadOnlyList<string> Collaborators,
    int VersionNumber,
    DateTimeOffset CreatedAt);
