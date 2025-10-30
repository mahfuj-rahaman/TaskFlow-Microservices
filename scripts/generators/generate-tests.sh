#!/bin/bash

################################################################################
# Tests Code Generator
################################################################################

generate_unit_tests() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local TESTS_PATH=$3

    local UNIT_TEST_FILE="$TESTS_PATH/UnitTests/TaskFlow.$SERVICE_NAME.UnitTests/Domain/${FEATURE_NAME}EntityTests.cs"

    print_info "Generating ${FEATURE_NAME}EntityTests.cs..."

    cat > "$UNIT_TEST_FILE" << EOF
using FluentAssertions;
using TaskFlow.$SERVICE_NAME.Domain.Entities;
using TaskFlow.$SERVICE_NAME.Domain.Exceptions;
using Xunit;

namespace TaskFlow.$SERVICE_NAME.UnitTests.Domain;

/// <summary>
/// Unit tests for ${FEATURE_NAME}Entity
/// </summary>
public class ${FEATURE_NAME}EntityTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreate${FEATURE_NAME}()
    {
        // Arrange
        var name = "Test ${FEATURE_NAME}";

        // Act
        var entity = ${FEATURE_NAME}Entity.Create(name);

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
        var act = () => ${FEATURE_NAME}Entity.Create(invalidName);

        // Assert
        act.Should().Throw<${SERVICE_NAME}DomainException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdate${FEATURE_NAME}()
    {
        // Arrange
        var entity = ${FEATURE_NAME}Entity.Create("Original Name");
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
        var entity = ${FEATURE_NAME}Entity.Create("Original Name");

        // Act
        var act = () => entity.Update(invalidName);

        // Assert
        act.Should().Throw<${SERVICE_NAME}DomainException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public void Delete_ShouldRaiseDomainEvent()
    {
        // Arrange
        var entity = ${FEATURE_NAME}Entity.Create("Test ${FEATURE_NAME}");
        entity.ClearDomainEvents(); // Clear creation event

        // Act
        entity.Delete();

        // Assert
        entity.DomainEvents.Should().HaveCount(1);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}EntityTests.cs"
}

generate_integration_tests() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local TESTS_PATH=$3

    local INTEGRATION_TEST_FILE="$TESTS_PATH/IntegrationTests/TaskFlow.$SERVICE_NAME.IntegrationTests/Api/${FEATURE_NAME}sControllerTests.cs"

    print_info "Generating ${FEATURE_NAME}sControllerTests.cs..."

    cat > "$INTEGRATION_TEST_FILE" << EOF
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.$SERVICE_NAME.Application.DTOs;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Create$FEATURE_NAME;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Update$FEATURE_NAME;
using Xunit;

namespace TaskFlow.$SERVICE_NAME.IntegrationTests.Api;

/// <summary>
/// Integration tests for ${FEATURE_NAME}sController
/// </summary>
public class ${FEATURE_NAME}sControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ${FEATURE_NAME}sControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll${FEATURE_NAME}s_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/${FEATURE_NAME}s");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create${FEATURE_NAME}_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new Create${FEATURE_NAME}Command
        {
            Name = "Test ${FEATURE_NAME}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/${FEATURE_NAME}s", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        result.Should().ContainKey("id");
        result!["id"].Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Create${FEATURE_NAME}_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new Create${FEATURE_NAME}Command
        {
            Name = "" // Invalid: empty name
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/${FEATURE_NAME}s", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get${FEATURE_NAME}ById_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        var createCommand = new Create${FEATURE_NAME}Command { Name = "Test ${FEATURE_NAME}" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/${FEATURE_NAME}s", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        // Act
        var response = await _client.GetAsync(\$"/api/v1/${FEATURE_NAME}s/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<${FEATURE_NAME}Dto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Test ${FEATURE_NAME}");
    }

    [Fact]
    public async Task Get${FEATURE_NAME}ById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync(\$"/api/v1/${FEATURE_NAME}s/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update${FEATURE_NAME}_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var createCommand = new Create${FEATURE_NAME}Command { Name = "Original Name" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/${FEATURE_NAME}s", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        var updateCommand = new Update${FEATURE_NAME}Command
        {
            Id = id,
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync(\$"/api/v1/${FEATURE_NAME}s/{id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete${FEATURE_NAME}_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var createCommand = new Create${FEATURE_NAME}Command { Name = "Test ${FEATURE_NAME}" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/${FEATURE_NAME}s", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        // Act
        var response = await _client.DeleteAsync(\$"/api/v1/${FEATURE_NAME}s/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete${FEATURE_NAME}_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync(\$"/api/v1/${FEATURE_NAME}s/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}sControllerTests.cs"
}
