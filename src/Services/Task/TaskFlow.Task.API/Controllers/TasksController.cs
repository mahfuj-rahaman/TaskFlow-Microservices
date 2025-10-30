using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Task.Application.Features.Tasks.Commands.CompleteTask;
using TaskFlow.Task.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Task.Application.Features.Tasks.Commands.UpdateTask;
using TaskFlow.Task.Application.Features.Tasks.Queries.GetAllTasks;
using TaskFlow.Task.Application.Features.Tasks.Queries.GetTaskById;

namespace TaskFlow.Task.API.Controllers;

/// <summary>
/// Tasks API Controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TasksController : ApiController
{
    public TasksController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tasks</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTasksQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="command">Create task command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = result.Value },
                new { id = result.Value });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="command">Update task command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        // Ensure ID from route matches command
        var updatedCommand = command with { TaskId = id };
        var result = await Mediator.Send(updatedCommand, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteTask(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTaskCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }
}
