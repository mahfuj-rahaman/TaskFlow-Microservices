using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Maps domain events to integration events for distributed publishing
/// </summary>
public interface IIntegrationEventMapper
{
    /// <summary>
    /// Maps a domain event to an integration event
    /// Returns null if the domain event should not be published to the message bus
    /// </summary>
    IIntegrationEvent? Map(IDomainEvent domainEvent);
}
