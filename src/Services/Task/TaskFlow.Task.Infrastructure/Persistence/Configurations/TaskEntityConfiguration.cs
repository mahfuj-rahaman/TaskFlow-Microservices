using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Task.Domain.Entities;
using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for TaskEntity
/// </summary>
public sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        // Table name
        builder.ToTable("Tasks");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .ValueGeneratedNever(); // We generate GUIDs in domain

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(20);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.CompletedAt);

        builder.Property(t => t.DueDate);

        // Indexes
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_Tasks_UserId");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tasks_Status");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_Tasks_CreatedAt");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("IX_Tasks_DueDate");

        // Ignore domain events (not persisted)
        builder.Ignore(t => t.DomainEvents);
    }
}
