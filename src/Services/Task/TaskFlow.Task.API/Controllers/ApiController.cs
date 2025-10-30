using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Task.API.Controllers;

/// <summary>
/// Base API controller with common functionality
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected ApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Returns an appropriate HTTP response based on the Result
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Returns an appropriate HTTP response based on the Result with value
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Maps error codes to HTTP status codes
    /// </summary>
    private IActionResult HandleFailure(Result result)
    {
        var errorCode = result.Error.Code;

        return errorCode switch
        {
            var code when code.Contains("NotFound") => NotFound(new { error = result.Error.Message }),
            var code when code.Contains("Validation") => BadRequest(new { error = result.Error.Message }),
            var code when code.Contains("Conflict") => Conflict(new { error = result.Error.Message }),
            _ => StatusCode(500, new { error = result.Error.Message })
        };
    }
}
