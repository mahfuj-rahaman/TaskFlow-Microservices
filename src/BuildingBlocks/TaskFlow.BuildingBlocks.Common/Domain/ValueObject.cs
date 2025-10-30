namespace TaskFlow.BuildingBlocks.Common.Domain;

/// <summary>
/// Base class for value objects
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that make up this value object
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject valueObject && Equals(valueObject);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Where(x => x != null)
            .Select(x => x!.GetHashCode())
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
