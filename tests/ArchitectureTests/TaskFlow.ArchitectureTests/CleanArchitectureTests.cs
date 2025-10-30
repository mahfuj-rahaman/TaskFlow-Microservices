using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace TaskFlow.ArchitectureTests;

/// <summary>
/// Architecture tests to enforce Clean Architecture principles
/// </summary>
public class CleanArchitectureTests
{
    private const string DomainNamespace = "TaskFlow.*.Domain";
    private const string ApplicationNamespace = "TaskFlow.*.Application";
    private const string InfrastructureNamespace = "TaskFlow.*.Infrastructure";
    private const string ApiNamespace = "TaskFlow.*.API";

    [Fact]
    public void Domain_Should_NotHaveDependencyOn_ApplicationLayer()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Domain");

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Application layer");
    }

    [Fact]
    public void Domain_Should_NotHaveDependencyOn_InfrastructureLayer()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Domain");

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void Domain_Should_NotHaveDependencyOn_ApiLayer()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Domain");

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on API layer");
    }

    [Fact]
    public void Application_Should_NotHaveDependencyOn_InfrastructureLayer()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Application");

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void Application_Should_NotHaveDependencyOn_ApiLayer()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Application");

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on API layer");
    }

    [Fact]
    public void Controllers_Should_HaveSuffixController()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.API");

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("TaskFlow.Task.API.Controllers")
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All controllers should end with 'Controller' suffix");
    }

    [Fact]
    public void Handlers_Should_HaveSuffixHandler()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Application");

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("TaskFlow.Task.Application.Features")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        // Note: This will pass even if no handlers exist yet
        result.IsSuccessful.Should().BeTrue(
            "All command/query handlers should end with 'Handler' suffix");
    }

    [Fact]
    public void Repositories_Should_HaveSuffixRepository()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Infrastructure");

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("TaskFlow.Task.Infrastructure.Repositories")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        // Note: This will pass even if no repositories exist yet
        result.IsSuccessful.Should().BeTrue(
            "All repositories should end with 'Repository' suffix");
    }

    [Fact]
    public void DomainEntities_Should_BeSealed_OrAbstract()
    {
        // Arrange
        var assembly = Assembly.Load("TaskFlow.Task.Domain");

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("TaskFlow.Task.Domain.Entities")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .Or()
            .BeAbstract()
            .GetResult();

        // Assert
        // Note: This will pass even if no entities exist yet
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should be either sealed or abstract to prevent improper inheritance");
    }

    [Fact]
    public void Infrastructure_Should_NotLeakTo_DomainLayer()
    {
        // Arrange
        var domainAssembly = Assembly.Load("TaskFlow.Task.Domain");

        // Act
        var result = Types.InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .And()
            .NotHaveDependencyOn("Npgsql")
            .And()
            .NotHaveDependencyOn("StackExchange.Redis")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have infrastructure dependencies");
    }
}
