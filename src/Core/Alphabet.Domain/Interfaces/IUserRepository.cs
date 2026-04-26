using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Provides application-user specific queries.
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken);

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
}
