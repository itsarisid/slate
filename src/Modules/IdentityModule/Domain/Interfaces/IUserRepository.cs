using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Provides application-user specific queries.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get by email async.
    /// </summary>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Get all async.
    /// </summary>

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Update async.
    /// </summary>

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
}
