using System.Linq.Expressions;

namespace Alphabet.Domain.Specifications;

/// <summary>
/// Base implementation for specifications.
/// </summary>
public abstract class BaseSpecification<T>(Expression<Func<T, bool>> criteria) : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];

    public Expression<Func<T, bool>> Criteria { get; } = criteria;

    public IReadOnlyCollection<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    protected void ApplyOrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
}
