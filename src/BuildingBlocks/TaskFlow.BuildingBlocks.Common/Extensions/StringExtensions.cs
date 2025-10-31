namespace TaskFlow.BuildingBlocks.Common.Extensions;

/// <summary>
/// Extension methods for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is null, empty, or whitespace
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    public static string ToCamelCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    public static string ToPascalCase(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Truncates a string to a specified length
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (value.IsNullOrEmpty() || value.Length <= maxLength)
            return value;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Checks if a string contains another string (case-insensitive)
    /// </summary>
    public static bool ContainsIgnoreCase(this string value, string substring)
    {
        if (value.IsNullOrEmpty() || substring.IsNullOrEmpty())
            return false;

        return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }
}
