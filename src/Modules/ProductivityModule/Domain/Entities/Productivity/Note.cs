using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a note with optional collaboration and version history.
/// </summary>
public sealed class Note : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public Guid? NotebookId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public NoteFormat Format { get; private set; }

    public string? Category { get; private set; }

    public string? Color { get; private set; }

    public bool IsPinned { get; private set; }

    public bool IsFavorite { get; private set; }

    public string CollaboratorsJson { get; private set; } = "[]";

    public string VersionHistoryJson { get; private set; } = "[]";

    public int VersionNumber { get; private set; } = 1;

    private Note()
    {
    }

    private Note(
        Guid ownerUserId,
        string title,
        string content,
        NoteFormat format,
        string? category,
        string? color,
        bool isPinned,
        bool isFavorite,
        Guid? notebookId,
        IReadOnlyCollection<string>? collaborators)
    {
        OwnerUserId = ownerUserId;
        Title = title.Trim();
        Content = content;
        Format = format;
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        IsPinned = isPinned;
        IsFavorite = isFavorite;
        NotebookId = notebookId;
        CollaboratorsJson = ProductivityJson.Serialize(collaborators);
    }
    /// <summary>
    /// Create.
    /// </summary>

    public static Note Create(
        Guid ownerUserId,
        string title,
        string content,
        NoteFormat format,
        string? category,
        string? color,
        bool isPinned,
        bool isFavorite,
        Guid? notebookId,
        IReadOnlyCollection<string>? collaborators)
        => new(ownerUserId, title, content, format, category, color, isPinned, isFavorite, notebookId, collaborators);

    public IReadOnlyList<string> Collaborators => ProductivityJson.DeserializeList<string>(CollaboratorsJson);

    public IReadOnlyList<NoteVersionSnapshot> Versions => ProductivityJson.DeserializeList<NoteVersionSnapshot>(VersionHistoryJson);
    /// <summary>
    /// Update.
    /// </summary>

    public void Update(string title, string content, NoteFormat format, string? category, string? color, bool isPinned, bool isFavorite, Guid? notebookId)
    {
        var versions = Versions.ToList();
        versions.Add(new NoteVersionSnapshot(VersionNumber, this.Content, DateTimeOffset.UtcNow));

        Title = title.Trim();
        Content = content;
        Format = format;
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        IsPinned = isPinned;
        IsFavorite = isFavorite;
        NotebookId = notebookId;
        VersionNumber += 1;
        VersionHistoryJson = ProductivityJson.Serialize<IReadOnlyCollection<NoteVersionSnapshot>>(versions);
        Touch();
    }
    /// <summary>
    /// Share with.
    /// </summary>

    public void ShareWith(string email)
    {
        var collaborators = Collaborators.ToHashSet(StringComparer.OrdinalIgnoreCase);
        collaborators.Add(email.Trim());
        CollaboratorsJson = ProductivityJson.Serialize<IReadOnlyCollection<string>>(collaborators.ToArray());
        Touch();
    }
}
