using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Common;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a base entity with auditing metadata.
/// </summary>
public abstract class BaseEntity : IAggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    private readonly List<DomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
