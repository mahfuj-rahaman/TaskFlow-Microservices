namespace TaskFlow.BuildingBlocks.Common.Time;

/// <summary>
/// Abstraction for system clock to enable testing
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current date (without time component)
    /// </summary>
    DateTime Today { get; }
}
