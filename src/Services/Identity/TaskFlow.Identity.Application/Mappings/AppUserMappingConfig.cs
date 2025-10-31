using Mapster;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Domain.Entities;

namespace TaskFlow.Identity.Application.Mappings;

/// <summary>
/// Mapster configuration for AppUser mappings
/// </summary>
public static class AppUserMappingConfig
{
    public static void Configure()
    {
        // AppUser to AppUserDto
        TypeAdapterConfig<AppUserEntity, AppUserDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
    }
}
