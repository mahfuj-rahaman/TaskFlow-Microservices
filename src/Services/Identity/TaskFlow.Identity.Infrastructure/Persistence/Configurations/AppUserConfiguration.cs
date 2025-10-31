using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Identity.Domain.Entities;

namespace TaskFlow.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AppUser
/// </summary>
public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUserEntity>
{
    public void Configure(EntityTypeBuilder<AppUserEntity> builder)
    {
        builder.ToTable("AppUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Name);

        // Ignore domain events (handled by base entity)
        builder.Ignore(x => x.DomainEvents);
    }
}
