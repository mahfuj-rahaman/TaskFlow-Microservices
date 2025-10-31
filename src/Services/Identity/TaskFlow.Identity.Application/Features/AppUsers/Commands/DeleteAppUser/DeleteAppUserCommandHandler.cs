using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.Interfaces;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.DeleteAppUser;

/// <summary>
/// Handler for DeleteAppUserCommand
/// </summary>
public sealed class DeleteAppUserCommandHandler : IRequestHandler<DeleteAppUserCommand, Result>
{
    private readonly IAppUserRepository _repository;

    public DeleteAppUserCommandHandler(IAppUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        DeleteAppUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "AppUser.NotFound",
                "AppUser not found"));
        }

        entity.Delete();

        await _repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
