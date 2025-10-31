using System.Linq.Expressions;

namespace TaskFlow.BuildingBlocks.Common.Specifications;

/// <summary>
/// Specification pattern for building complex queries
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The filter criteria
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Navigation properties to include
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// String-based includes (for nested includes)
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Number of items to take
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of items to skip
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Whether to track changes (for read-only queries)
    /// </summary>
    bool AsNoTracking { get; }
}
