using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Persistence.Context;

namespace Alphabet.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core unit-of-work adapter.
/// </summary>
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
