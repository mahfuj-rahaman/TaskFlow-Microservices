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

        // Basic identity properties
        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        // Email confirmation
        builder.Property(x => x.EmailConfirmed)
            .IsRequired();

        builder.Property(x => x.EmailConfirmationToken)
            .HasMaxLength(500);

        builder.Property(x => x.PasswordResetToken)
            .HasMaxLength(500);

        // Security properties
        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FailedLoginAttempts)
            .IsRequired();

        builder.Property(x => x.LastLoginIp)
            .HasMaxLength(50);

        // Collections stored as JSON
        builder.Property(x => x.Roles)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Permissions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Two-factor
        builder.Property(x => x.TwoFactorEnabled)
            .IsRequired();

        builder.Property(x => x.TwoFactorSecret)
            .HasMaxLength(100);

        // Audit
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        // Ignore domain events (handled by base entity)
        builder.Ignore(x => x.DomainEvents);
    }
}
