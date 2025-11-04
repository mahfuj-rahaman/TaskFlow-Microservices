using MediatR;
using TaskFlow.Identity.Application.DTOs;

namespace TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAppUserById;

/// <summary>
/// Query to get a AppUser by ID
/// </summary>
public sealed record GetAppUserByIdQuery(Guid Id) : IRequest<AppUserDto?>;
