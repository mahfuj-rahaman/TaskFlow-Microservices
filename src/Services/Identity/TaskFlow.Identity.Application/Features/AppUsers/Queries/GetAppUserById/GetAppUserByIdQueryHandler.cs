using Mapster;
using MediatR;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Interfaces;

namespace TaskFlow.Identity.Application.Features.AppUsers.Queries.GetAppUserById;

/// <summary>
/// Handler for GetAppUserByIdQuery
/// </summary>
public sealed class GetAppUserByIdQueryHandler : IRequestHandler<GetAppUserByIdQuery, AppUserDto?>
{
    private readonly IAppUserRepository _repository;

    public GetAppUserByIdQueryHandler(IAppUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppUserDto?> Handle(
        GetAppUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return entity?.Adapt<AppUserDto>();
    }
}
