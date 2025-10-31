using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

public sealed record AppUserEmailConfirmedDomainEvent(
    Guid AppUserId,
    string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserLoggedInDomainEvent(
    Guid AppUserId,
    string Username,
    string IpAddress) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserLockedOutDomainEvent(
    Guid AppUserId,
    string Username,
    DateTime LockoutEnd) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserPasswordChangedDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserPasswordResetRequestedDomainEvent(
    Guid AppUserId,
    string Email,
    string ResetToken) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserPasswordResetDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserRoleAddedDomainEvent(
    Guid AppUserId,
    string Username,
    string Role) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserRoleRemovedDomainEvent(
    Guid AppUserId,
    string Username,
    string Role) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserTwoFactorEnabledDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserTwoFactorDisabledDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserDeactivatedDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserActivatedDomainEvent(
    Guid AppUserId,
    string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record AppUserSuspendedDomainEvent(
    Guid AppUserId,
    string Username,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
