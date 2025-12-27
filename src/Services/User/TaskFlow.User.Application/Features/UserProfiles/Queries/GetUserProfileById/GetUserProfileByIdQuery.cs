using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.User.Application.DTOs;

namespace TaskFlow.User.Application.Features.UserProfiles.Queries.GetUserProfileById;

public sealed record GetUserProfileByIdQuery(Guid Id) : IRequest<Result<UserProfileDto>>;
