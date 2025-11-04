using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;
using TaskFlow.Identity.Application.Features.AppUsers.Commands.UpdateAppUser;
using Xunit;

namespace TaskFlow.Identity.IntegrationTests.Api;

/// <summary>
/// Integration tests for AppUsersController
/// </summary>
public class AppUsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AppUsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAppUsers_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/AppUsers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAppUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new CreateAppUserCommand
        {
            Name = "Test AppUser"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/AppUsers", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        result.Should().ContainKey("id");
        result!["id"].Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAppUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateAppUserCommand
        {
            Name = "" // Invalid: empty name
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/AppUsers", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppUserById_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        var createCommand = new CreateAppUserCommand { Name = "Test AppUser" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/AppUsers", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        // Act
        var response = await _client.GetAsync($"/api/v1/AppUsers/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppUserDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Test AppUser");
    }

    [Fact]
    public async Task GetAppUserById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/AppUsers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAppUser_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var createCommand = new CreateAppUserCommand { Name = "Original Name" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/AppUsers", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        var updateCommand = new UpdateAppUserCommand
        {
            Id = id,
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/AppUsers/{id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAppUser_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var createCommand = new CreateAppUserCommand { Name = "Test AppUser" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/AppUsers", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = createResult!["id"];

        // Act
        var response = await _client.DeleteAsync($"/api/v1/AppUsers/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAppUser_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/AppUsers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
