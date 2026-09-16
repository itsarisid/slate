namespace Alphabet.Domain.Specifications;

/// <summary>
/// Applies specification criteria and ordering to an <see cref="IQueryable{T}" />.
/// Infrastructure repositories remain responsible for provider-specific includes.
/// </summary>
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery.Where(specification.Criteria);

        return specification.OrderBy is null
            ? query
            : specification.OrderBy(query);
    }
}
