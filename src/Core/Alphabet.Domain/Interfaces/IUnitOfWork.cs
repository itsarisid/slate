namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Defines a unit of work contract for transactional persistence.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes to the underlying store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
