using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.DeleteAppUser;

/// <summary>
/// Command to delete a AppUser
/// </summary>
public sealed record DeleteAppUserCommand(Guid Id) : IRequest<Result>;
