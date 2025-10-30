namespace TaskFlow.BuildingBlocks.Common.Results;

/// <summary>
/// Represents an error
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    public static implicit operator string(Error error) => error.Code;
}
