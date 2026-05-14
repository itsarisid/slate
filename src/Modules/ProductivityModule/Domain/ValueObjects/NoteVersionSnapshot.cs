using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a stored note version snapshot.
/// </summary>
[NotMapped]
public sealed record NoteVersionSnapshot(int VersionNumber, string Content, DateTimeOffset SavedAt);
