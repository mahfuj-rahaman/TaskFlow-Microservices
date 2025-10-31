namespace TaskFlow.BuildingBlocks.Common.Time;

/// <summary>
/// Production implementation of system clock
/// </summary>
public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;

    public DateTime Today => DateTime.Today;
}
