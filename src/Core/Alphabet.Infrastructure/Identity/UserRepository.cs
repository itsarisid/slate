using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Identity;

/// <summary>
/// Provides custom user queries on top of the Identity store.
/// </summary>
public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => dbContext.UsersOfIdentity().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.UsersOfIdentity().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken)
        => await dbContext.UsersOfIdentity().OrderBy(x => x.Email).ToListAsync(cancellationToken);

    public Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        dbContext.UsersOfIdentity().Update(user);
        return Task.CompletedTask;
    }
}
