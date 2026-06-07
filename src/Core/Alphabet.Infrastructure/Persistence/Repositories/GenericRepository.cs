using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;

namespace Alphabet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Named generic repository for new module implementations.
/// </summary>
public class GenericRepository<T>(AppDbContext dbContext) : Repository<T>(dbContext)
    where T : BaseEntity;
