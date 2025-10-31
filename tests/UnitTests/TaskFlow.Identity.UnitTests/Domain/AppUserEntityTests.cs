using FluentAssertions;
using TaskFlow.Identity.Domain.Entities;
using TaskFlow.Identity.Domain.Exceptions;
using Xunit;

namespace TaskFlow.Identity.UnitTests.Domain;

/// <summary>
/// Unit tests for AppUserEntity
/// </summary>
public class AppUserEntityTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAppUser()
    {
        // Arrange
        var name = "Test AppUser";

        // Act
        var entity = AppUserEntity.Create(name);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBe(Guid.Empty);
        entity.Name.Should().Be(name);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeNull();
        entity.DomainEvents.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Act
        var act = () => AppUserEntity.Create(invalidName);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAppUser()
    {
        // Arrange
        var entity = AppUserEntity.Create("Original Name");
        var newName = "Updated Name";

        // Act
        entity.Update(newName);

        // Assert
        entity.Name.Should().Be(newName);
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var entity = AppUserEntity.Create("Original Name");

        // Act
        var act = () => entity.Update(invalidName);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public void Delete_ShouldRaiseDomainEvent()
    {
        // Arrange
        var entity = AppUserEntity.Create("Test AppUser");
        entity.ClearDomainEvents(); // Clear creation event

        // Act
        entity.Delete();

        // Assert
        entity.DomainEvents.Should().HaveCount(1);
    }
}
