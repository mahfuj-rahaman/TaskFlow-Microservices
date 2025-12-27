using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Features.Auth.Commands.Login;
using TaskFlow.Identity.Application.Features.Auth.Commands.Register;

namespace TaskFlow.Identity.API.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = request.Adapt<RegisterCommand>();
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    /// <summary>
    /// Login a user
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error.Code == "Auth.AccountLocked")
            {
                return StatusCode(StatusCodes.Status423Locked, new { error = result.Error.Message });
            }

            return Unauthorized(new { error = result.Error.Message });
        }

        return Ok(result.Value);
    }
}
