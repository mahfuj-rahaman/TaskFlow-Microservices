using MediatR;
using TaskFlow.Identity.Application.DTOs;

namespace TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAllAppUsers;

/// <summary>
/// Query to get all AppUsers
/// </summary>
public sealed record GetAllAppUsersQuery : IRequest<IReadOnlyList<AppUserDto>>;
