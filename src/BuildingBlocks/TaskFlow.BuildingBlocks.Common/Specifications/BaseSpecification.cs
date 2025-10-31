using System.Linq.Expressions;

namespace TaskFlow.BuildingBlocks.Common.Specifications;

/// <summary>
/// Base implementation of the specification pattern
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled => Skip.HasValue && Take.HasValue;
    public bool AsNoTracking { get; private set; }

    /// <summary>
    /// Adds a navigation property to include in the query
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include (for nested includes like "Author.Books")
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Applies paging to the query
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Applies ordering to the query
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Applies descending ordering to the query
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Disables change tracking for read-only queries
    /// </summary>
    protected void ApplyAsNoTracking()
    {
        AsNoTracking = true;
    }
}
