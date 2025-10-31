using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;
using TaskFlow.Identity.Application.Features.AppUsers.Commands.UpdateAppUser;
using TaskFlow.Identity.Application.Features.AppUsers.Commands.DeleteAppUser;
using TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAppUserById;
using TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAllAppUsers;

namespace TaskFlow.Identity.API.Controllers;

/// <summary>
/// Controller for AppUser operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AppUsersController : ApiController
{
    private readonly ISender _sender;

    public AppUsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all AppUsers
    /// </summary>
    /// <response code="200">Returns the list of AppUsers</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppUsers(CancellationToken cancellationToken)
    {
        var query = new GetAllAppUsersQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get AppUser by ID
    /// </summary>
    /// <param name="id">The AppUser ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns the AppUser</response>
    /// <response code="404">AppUser not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAppUserByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "AppUser not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new AppUser
    /// </summary>
    /// <param name="command">The create command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">AppUser created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppUser(
        [FromBody] CreateAppUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result, id => CreatedAtAction(
            nameof(GetAppUserById),
            new { id },
            new { id }));
    }

    /// <summary>
    /// Update an existing AppUser
    /// </summary>
    /// <param name="id">The AppUser ID</param>
    /// <param name="command">The update command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">AppUser updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">AppUser not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppUser(
        Guid id,
        [FromBody] UpdateAppUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Delete a AppUser
    /// </summary>
    /// <param name="id">The AppUser ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">AppUser deleted successfully</response>
    /// <response code="404">AppUser not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppUser(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAppUserCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
