namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a notebook container for notes.
/// </summary>
public sealed class Notebook : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Color { get; private set; }

    public Guid? ParentNotebookId { get; private set; }

    private Notebook()
    {
    }

    private Notebook(Guid ownerUserId, string name, string? description, string? color, Guid? parentNotebookId)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Description = description?.Trim();
        Color = color?.Trim();
        ParentNotebookId = parentNotebookId;
    }

    public static Notebook Create(Guid ownerUserId, string name, string? description, string? color, Guid? parentNotebookId)
        => new(ownerUserId, name, description, color, parentNotebookId);
}
