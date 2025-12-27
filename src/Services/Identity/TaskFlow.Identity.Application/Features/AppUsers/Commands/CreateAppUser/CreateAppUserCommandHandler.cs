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
    private readonly IPasswordHasher _passwordHasher;

    public CreateAppUserCommandHandler(
        IAppUserRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(
        CreateAppUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _repository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>(
                new Error("AppUser.EmailExists", $"Email '{request.Email}' already exists"));
        }

        // Check if username already exists
        if (await _repository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            return Result.Failure<Guid>(
                new Error("AppUser.UsernameExists", $"Username '{request.Username}' already exists"));
        }

        // Hash password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create entity
        var entity = AppUserEntity.Create(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash);

        await _repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id);
    }
}
