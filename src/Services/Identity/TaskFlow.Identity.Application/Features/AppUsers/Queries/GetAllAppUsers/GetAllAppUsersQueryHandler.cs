using Mapster;
using MediatR;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Interfaces;

namespace TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAllAppUsers;

/// <summary>
/// Handler for GetAllAppUsersQuery
/// </summary>
public sealed class GetAllAppUsersQueryHandler : IRequestHandler<GetAllAppUsersQuery, IReadOnlyList<AppUserDto>>
{
    private readonly IAppUserRepository _repository;

    public GetAllAppUsersQueryHandler(IAppUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AppUserDto>> Handle(
        GetAllAppUsersQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return entities.Adapt<IReadOnlyList<AppUserDto>>();
    }
}
