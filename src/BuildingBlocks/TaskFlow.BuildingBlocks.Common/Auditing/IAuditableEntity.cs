namespace TaskFlow.BuildingBlocks.Common.Auditing;

/// <summary>
/// Interface for entities that track creation and modification audit information
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// When the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created the entity
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated the entity
    /// </summary>
    string? UpdatedBy { get; set; }
}
