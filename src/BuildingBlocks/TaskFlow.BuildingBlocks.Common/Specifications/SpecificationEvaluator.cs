using Microsoft.EntityFrameworkCore;

namespace TaskFlow.BuildingBlocks.Common.Specifications;

/// <summary>
/// Evaluates specifications and applies them to IQueryable
/// </summary>
public static class SpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Applies the specification to the input query
    /// </summary>
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply AsNoTracking if specified
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply criteria (WHERE clause)
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes (eager loading)
        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings.Aggregate(
            query,
            (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip!.Value)
                         .Take(specification.Take!.Value);
        }

        return query;
    }
}
