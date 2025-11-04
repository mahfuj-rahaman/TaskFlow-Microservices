using Microsoft.AspNetCore.Mvc;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Admin.API.Controllers;

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
