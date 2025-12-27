using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Events;
using TaskFlow.User.Domain.Enums;

namespace TaskFlow.User.Domain.Entities;

/// <summary>
/// UserProfile aggregate root
/// </summary>
public sealed class UserProfileEntity : AggregateRoot<Guid>
{
    public Guid AppUserId { get; private set; } // Reference to Identity service AppUser
    public string DisplayName { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public string Timezone { get; private set; } = "UTC";
    public string Language { get; private set; } = "en";
    public UserProfileStatus Status { get; private set; } = UserProfileStatus.Active;
    public Dictionary<string, string> SocialLinks { get; private set; } = new();
    public Dictionary<string, string> Preferences { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private UserProfileEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new UserProfile
    /// </summary>
    public static UserProfileEntity Create(Guid appUserId, string displayName)
    {
        var entity = new UserProfileEntity(Guid.NewGuid())
        {
            AppUserId = appUserId,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            Status = UserProfileStatus.Active,
            SocialLinks = new Dictionary<string, string>(),
            Preferences = new Dictionary<string, string>()
        };

        entity.RaiseDomainEvent(new UserProfileCreatedDomainEvent(entity.Id, appUserId));

        return entity;
    }

    /// <summary>
    /// Updates profile information
    /// </summary>
    public void UpdateProfile(
        string displayName,
        string? bio,
        string? phoneNumber)
    {
        DisplayName = displayName;
        Bio = bio;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates address information
    /// </summary>
    public void UpdateAddress(
        string? address,
        string? city,
        string? country,
        string? postalCode)
    {
        Address = address;
        City = city;
        Country = country;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates avatar
    /// </summary>
    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates preferences
    /// </summary>
    public void UpdatePreferences(string timezone, string language)
    {
        Timezone = timezone;
        Language = language;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds or updates a social link
    /// </summary>
    public void SetSocialLink(string platform, string url)
    {
        SocialLinks[platform] = url;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a social link
    /// </summary>
    public void RemoveSocialLink(string platform)
    {
        SocialLinks.Remove(platform);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets a preference value
    /// </summary>
    public void SetPreference(string key, string value)
    {
        Preferences[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a preference
    /// </summary>
    public void RemovePreference(string key)
    {
        Preferences.Remove(key);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes profile status
    /// </summary>
    public void ChangeStatus(UserProfileStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deletes the profile
    /// </summary>
    public void Delete()
    {
        Status = UserProfileStatus.Deleted;
        RaiseDomainEvent(new UserProfileDeletedDomainEvent(Id, AppUserId));
    }
}
