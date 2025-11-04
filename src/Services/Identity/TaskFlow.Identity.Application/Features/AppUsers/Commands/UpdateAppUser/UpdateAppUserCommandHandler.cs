using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.Interfaces;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.UpdateAppUser;

/// <summary>
/// Handler for UpdateAppUserCommand
/// </summary>
public sealed class UpdateAppUserCommandHandler : IRequestHandler<UpdateAppUserCommand, Result>
{
    private readonly IAppUserRepository _repository;

    public UpdateAppUserCommandHandler(IAppUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        UpdateAppUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "AppUser.NotFound",
                "AppUser not found"));
        }

        // TODO: Customize entity update with actual properties from request
        entity.Update();

        await _repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
