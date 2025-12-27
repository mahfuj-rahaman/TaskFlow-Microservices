namespace TaskFlow.Task.Application.DTOs;

public sealed record TaskItemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
