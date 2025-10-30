#!/bin/bash

################################################################################
# API Layer Code Generator
################################################################################

generate_controller() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local API_PATH=$3

    local CONTROLLER_FILE="$API_PATH/Controllers/${FEATURE_NAME}sController.cs"

    print_info "Generating ${FEATURE_NAME}sController.cs..."

    cat > "$CONTROLLER_FILE" << EOF
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.$SERVICE_NAME.Application.DTOs;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Create$FEATURE_NAME;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Update$FEATURE_NAME;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Delete$FEATURE_NAME;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.Get${FEATURE_NAME}ById;
using TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.GetAll${FEATURE_NAME}s;

namespace TaskFlow.$SERVICE_NAME.API.Controllers;

/// <summary>
/// Controller for $FEATURE_NAME operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ${FEATURE_NAME}sController : ApiController
{
    private readonly ISender _sender;

    public ${FEATURE_NAME}sController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all ${FEATURE_NAME}s
    /// </summary>
    /// <response code="200">Returns the list of ${FEATURE_NAME}s</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<${FEATURE_NAME}Dto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll${FEATURE_NAME}s(CancellationToken cancellationToken)
    {
        var query = new GetAll${FEATURE_NAME}sQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get $FEATURE_NAME by ID
    /// </summary>
    /// <param name="id">The $FEATURE_NAME ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns the $FEATURE_NAME</response>
    /// <response code="404">$FEATURE_NAME not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(${FEATURE_NAME}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get${FEATURE_NAME}ById(Guid id, CancellationToken cancellationToken)
    {
        var query = new Get${FEATURE_NAME}ByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "$FEATURE_NAME not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new $FEATURE_NAME
    /// </summary>
    /// <param name="command">The create command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">$FEATURE_NAME created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create${FEATURE_NAME}(
        [FromBody] Create${FEATURE_NAME}Command command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result, id => CreatedAtAction(
            nameof(Get${FEATURE_NAME}ById),
            new { id },
            new { id }));
    }

    /// <summary>
    /// Update an existing $FEATURE_NAME
    /// </summary>
    /// <param name="id">The $FEATURE_NAME ID</param>
    /// <param name="command">The update command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">$FEATURE_NAME updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">$FEATURE_NAME not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update${FEATURE_NAME}(
        Guid id,
        [FromBody] Update${FEATURE_NAME}Command command,
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
    /// Delete a $FEATURE_NAME
    /// </summary>
    /// <param name="id">The $FEATURE_NAME ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">$FEATURE_NAME deleted successfully</response>
    /// <response code="404">$FEATURE_NAME not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete${FEATURE_NAME}(Guid id, CancellationToken cancellationToken)
    {
        var command = new Delete${FEATURE_NAME}Command(id);
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}sController.cs"

    # Check if ApiController base class exists
    local BASE_CONTROLLER="$API_PATH/Controllers/ApiController.cs"
    if [ ! -f "$BASE_CONTROLLER" ]; then
        print_info "Generating ApiController base class..."

        cat > "$BASE_CONTROLLER" << EOF
using Microsoft.AspNetCore.Mvc;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$SERVICE_NAME.API.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke(result.Value) ?? Ok(result.Value);
        }

        return HandleFailure(result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleFailure(result);
    }

    private IActionResult HandleFailure(Result result)
    {
        return result.Error.Code switch
        {
            _ when result.Error.Code.EndsWith(".NotFound") => NotFound(new { error = result.Error.Message }),
            _ when result.Error.Code.EndsWith(".Validation") => BadRequest(new { error = result.Error.Message }),
            _ => StatusCode(500, new { error = "An error occurred processing your request" })
        };
    }
}
EOF

        print_success "Created ApiController base class"
    fi
}
