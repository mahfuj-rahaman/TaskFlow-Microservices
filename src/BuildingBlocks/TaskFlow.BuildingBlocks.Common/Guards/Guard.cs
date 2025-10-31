namespace TaskFlow.BuildingBlocks.Common.Guards;

/// <summary>
/// Provides guard clauses for method arguments
/// </summary>
public static class Guard
{
    /// <summary>
    /// Guards against null values
    /// </summary>
    public static void AgainstNull<T>(T value, string parameterName, string? message = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName, message ?? $"{parameterName} cannot be null");
        }
    }

    /// <summary>
    /// Guards against null or empty strings
    /// </summary>
    public static void AgainstNullOrEmpty(string? value, string parameterName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                message ?? $"{parameterName} cannot be null or empty",
                parameterName);
        }
    }

    /// <summary>
    /// Guards against negative or zero values
    /// </summary>
    public static void AgainstNegativeOrZero(int value, string parameterName, string? message = null)
    {
        if (value <= 0)
        {
            throw new ArgumentException(
                message ?? $"{parameterName} must be positive",
                parameterName);
        }
    }

    /// <summary>
    /// Guards against negative values
    /// </summary>
    public static void AgainstNegative(int value, string parameterName, string? message = null)
    {
        if (value < 0)
        {
            throw new ArgumentException(
                message ?? $"{parameterName} cannot be negative",
                parameterName);
        }
    }

    /// <summary>
    /// Guards against values outside a specified range
    /// </summary>
    public static void AgainstOutOfRange(int value, int min, int max, string parameterName, string? message = null)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                message ?? $"{parameterName} must be between {min} and {max}");
        }
    }

    /// <summary>
    /// Guards against empty collections
    /// </summary>
    public static void AgainstEmptyCollection<T>(IEnumerable<T>? collection, string parameterName, string? message = null)
    {
        if (collection is null || !collection.Any())
        {
            throw new ArgumentException(
                message ?? $"{parameterName} cannot be empty",
                parameterName);
        }
    }

    /// <summary>
    /// Guards against invalid enum values
    /// </summary>
    public static void AgainstInvalidEnum<TEnum>(TEnum value, string parameterName, string? message = null)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            throw new ArgumentException(
                message ?? $"{parameterName} has an invalid value",
                parameterName);
        }
    }
}
