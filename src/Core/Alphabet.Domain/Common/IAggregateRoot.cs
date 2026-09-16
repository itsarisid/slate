namespace Alphabet.Domain.Common;

/// <summary>
/// Marks an entity as an aggregate root that can raise domain events.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
