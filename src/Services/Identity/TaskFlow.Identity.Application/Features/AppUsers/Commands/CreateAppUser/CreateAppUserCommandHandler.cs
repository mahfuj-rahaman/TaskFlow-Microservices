using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.Interfaces;
using TaskFlow.Identity.Domain.Entities;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;

/// <summary>
/// Handler for CreateAppUserCommand
/// </summary>
public sealed class CreateAppUserCommandHandler : IRequestHandler<CreateAppUserCommand, Result<Guid>>
{
    private readonly IAppUserRepository _repository;

    public CreateAppUserCommandHandler(IAppUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(
        CreateAppUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = AppUserEntity.Create(request.Name);

        await _repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id);
    }
}
