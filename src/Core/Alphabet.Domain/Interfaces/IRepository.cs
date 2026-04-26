using Alphabet.Domain.Entities;
using Alphabet.Domain.Specifications;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Defines a generic repository contract.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Marks an entity as updated.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for removal.
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Finds entities using a specification.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken);
}
