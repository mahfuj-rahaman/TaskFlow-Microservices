# Common BuildingBlocks Specification

**Project**: TaskFlow.BuildingBlocks.Common
**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Status**: Specification

---

## 📋 Overview

The Common BuildingBlocks provide shared abstractions, patterns, and utilities used across all microservices.

## 🎯 Current Components (Already Implemented)

### ✅ Domain
- `Entity<TId>` - Base entity with domain events
- `AggregateRoot<TId>` - Aggregate root marker
- `ValueObject` - Value object base class
- `IDomainEvent` - Domain event interface

### ✅ Results
- `Result` - Operation result without value
- `Result<T>` - Operation result with value
- `Error` - Error representation

### ✅ Exceptions
- `DomainException` - Base domain exception

## 📦 Missing Components to Implement

### 1. Pagination

#### PagedList<T>
```csharp
public sealed class PagedList<T>
{
    public List<T> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedList<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedList<T>
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
```

#### PaginationExtensions
```csharp
public static class PaginationExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedList<T>.Create(items, pageNumber, pageSize, totalCount);
    }
}
```

### 2. Repository Base Interfaces

#### IRepository<TEntity, TId>
```csharp
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedList<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
```

### 3. Specification Pattern

#### ISpecification<T>
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}

public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>>? criteria)
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

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
}
```

#### SpecificationEvaluator
```csharp
public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(
            query,
            (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip!.Value)
                         .Take(specification.Take!.Value);
        }

        return query;
    }
}
```

### 4. Audit Properties

#### IAuditableEntity
```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

### 5. Guard Clauses

#### Guard
```csharp
public static class Guard
{
    public static void AgainstNull<T>(T value, string parameterName)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", parameterName);
    }

    public static void AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be positive", parameterName);
    }

    public static void AgainstOutOfRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"Value must be between {min} and {max}");
    }
}
```

### 6. Extension Methods

#### StringExtensions
```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrWhiteSpace(value);

    public static string ToCamelCase(this string value)
        => char.ToLowerInvariant(value[0]) + value[1..];

    public static string ToPascalCase(this string value)
        => char.ToUpperInvariant(value[0]) + value[1..];
}
```

#### EnumerableExtensions
```csharp
public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        => source is null || !source.Any();

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source.Where(x => x is not null)!;
}
```

### 7. Clock Abstraction

#### ISystemClock
```csharp
public interface ISystemClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
```

## 📝 Usage Examples

### Pagination
```csharp
public async Task<PagedList<ProductDto>> Handle(
    GetProductsQuery request,
    CancellationToken ct)
{
    var query = _context.Products.AsQueryable();

    var pagedProducts = await query.ToPagedListAsync(
        request.PageNumber,
        request.PageSize,
        ct);

    return new PagedList<ProductDto>
    {
        Items = pagedProducts.Items.Adapt<List<ProductDto>>(),
        PageNumber = pagedProducts.PageNumber,
        PageSize = pagedProducts.PageSize,
        TotalCount = pagedProducts.TotalCount
    };
}
```

### Specification Pattern
```csharp
public sealed class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
        : base(p => p.IsActive && !p.IsDeleted)
    {
        AddInclude(p => p.Category);
        ApplyOrderBy(p => p.Name);
    }
}

// Usage
var spec = new ActiveProductsSpecification();
var query = SpecificationEvaluator<Product>.GetQuery(_context.Products, spec);
var products = await query.ToListAsync(ct);
```

## 🔧 Dependencies

- `Microsoft.EntityFrameworkCore` (8.0.10)
- `System.Linq.Expressions` (built-in)

## ✅ Implementation Checklist

- [x] Domain (Entity, AggregateRoot, ValueObject, IDomainEvent)
- [x] Results (Result, Error)
- [x] Exceptions (DomainException)
- [ ] Pagination (PagedList, Extensions)
- [ ] Repository Interfaces (IRepository)
- [ ] Specification Pattern (ISpecification, BaseSpecification, Evaluator)
- [ ] Audit Properties (IAuditableEntity, ISoftDeletable)
- [ ] Guard Clauses
- [ ] Extension Methods (String, Enumerable)
- [ ] Clock Abstraction (ISystemClock)

## 🎯 Success Criteria

1. ✅ All services can use common patterns
2. ✅ Reduced code duplication across services
3. ✅ Consistent pagination across APIs
4. ✅ Type-safe query building with specifications
5. ✅ Standardized audit trail

---

**Summary**: Complete Common BuildingBlocks before implementing services
