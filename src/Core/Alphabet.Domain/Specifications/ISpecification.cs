using System.Linq.Expressions;

namespace Alphabet.Domain.Specifications;

/// <summary>
/// Defines a reusable query specification.
/// </summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }

    IReadOnlyCollection<Expression<Func<T, object>>> Includes { get; }

    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
}
