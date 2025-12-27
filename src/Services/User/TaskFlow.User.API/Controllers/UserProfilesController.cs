using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.User.Application.DTOs;
using TaskFlow.User.Application.Features.UserProfiles.Commands.CreateUserProfile;
using TaskFlow.User.Application.Features.UserProfiles.Commands.UpdateUserProfile;
using TaskFlow.User.Application.Features.UserProfiles.Queries.GetUserProfileById;

namespace TaskFlow.User.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserProfileRequest request)
    {
        var command = request.Adapt<CreateUserProfileCommand>();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserProfileRequest request)
    {
        var command = new UpdateUserProfileCommand
        {
            Id = id,
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            PhoneNumber = request.PhoneNumber
        };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserProfileByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }
}
