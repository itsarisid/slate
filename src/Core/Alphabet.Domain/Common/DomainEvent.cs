using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.Common;

/// <summary>
/// Base type for domain events raised by aggregates.
/// </summary>
[NotMapped]
public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
