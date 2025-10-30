using Mapster;
using TaskFlow.Task.Application.DTOs;
using TaskFlow.Task.Domain.Entities;

namespace TaskFlow.Task.Application.Mappings;

/// <summary>
/// Mapster configuration for Task mappings
/// </summary>
public static class TaskMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<TaskEntity, TaskDto>.NewConfig()
            .Map(dest => dest.IsOverdue, src => src.IsOverdue());
    }
}
