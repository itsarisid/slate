using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Specifications;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides a generic EF Core repository implementation.
/// </summary>
public class Repository<T>(AppDbContext dbContext) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext DbContext = dbContext;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<T>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await DbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }

    public void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }

    public async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        IQueryable<T> query = DbContext.Set<T>();
        query = query.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        if (specification.OrderBy is not null)
        {
            query = specification.OrderBy(query);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
