using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents a task checklist item.
/// </summary>
[NotMapped]
public sealed record TodoChecklistItem(string Text, bool Completed);
