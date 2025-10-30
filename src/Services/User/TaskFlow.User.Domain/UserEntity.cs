using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Enums;
using TaskFlow.User.Domain.Events;
using TaskFlow.User.Domain.Exceptions;

namespace TaskFlow.User.Domain.Entities;

/// <summary>
/// User aggregate root representing a system user
/// </summary>
public sealed class UserEntity : AggregateRoot<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private UserEntity(
        Guid id, string email, string firstName, string lastName, DateTime dateOfBirth, UserStatus status) : base(id) // <-- Pass id to AggregateRoot<Guid> base constructor
    {
        this.Email = email;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.DateOfBirth = dateOfBirth;
        this.Status = status;

        RaiseDomainEvent(new UserCreatedDomainEvent(this.Id, this.Email, this.FirstName, this.LastName, this.DateOfBirth, this.Status));
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public static UserEntity Create(
        string email,
        string firstName,
        string lastName,
        DateTime dateOfBirth)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            throw new UserDomainException("Email is required");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new UserDomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new UserDomainException("Last name is required");

        if (dateOfBirth >= DateTime.Today)
            throw new UserDomainException("Date of birth must be in the past");

        if (dateOfBirth > DateTime.Today.AddYears(-18))
            throw new UserDomainException("User must be at least 18 years old");

        // Use the private constructor with all required parameters
        var user = new UserEntity(
            Guid.NewGuid(),
            email.ToLowerInvariant(),
            firstName,
            lastName,
            dateOfBirth,
            UserStatus.Active
        );

        user.CreatedAt = DateTime.UtcNow;

        return user;
    }

    /// <summary>
    /// Updates user information
    /// </summary>
    public void Update(string firstName, string lastName, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new UserDomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new UserDomainException("Last name is required");

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            throw new UserDomainException("User is already inactive");

        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserDeactivatedDomainEvent(Id));
    }

    /// <summary>
    /// Activates the user
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new UserDomainException("User is already active");

        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records user login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets full name
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>
    /// Calculates age
    /// </summary>
    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}