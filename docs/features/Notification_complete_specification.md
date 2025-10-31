# Notification Feature - Complete Specification

## 📋 Feature Overview

**Feature Name**: Notification
**Service**: Notification Service
**Purpose**: Production-ready notification system handling real-time and asynchronous notifications for task updates, user invitations, admin actions, and system events across multiple channels (email, SMS, in-app, push notifications)

---

## 🎯 Core Requirements

### Notification Triggers

**Task-Related Notifications**:
- Task created
- Task assigned to user
- Task reassigned
- Task status changed (InProgress, UnderReview, Completed, Blocked, Cancelled)
- Task commented
- Task review requested
- Task review completed (approved/rejected)
- Task dismissed by admin
- Task suspended by admin
- Task deadline approaching (24 hours, 1 hour before)
- Task overdue

**User/Invitation Notifications**:
- User registered (welcome email)
- Email confirmation required
- Email confirmed successfully
- Master user sent SubUser invitation
- SubUser invitation received (for existing users)
- SubUser invitation received (for new users - must register first)
- SubUser accepted invitation
- SubUser rejected invitation
- Invitation expired
- User promoted to Admin
- User demoted from Admin

**Admin Action Notifications**:
- User blocked by admin
- User unblocked by admin
- User suspended by admin
- User activated by admin
- Account locked (failed login attempts)

**System Notifications**:
- Password reset requested
- Password changed successfully
- Two-factor authentication enabled
- Security alert (login from new device/location)

### Notification Channels
- **Email**: Full HTML emails with templates
- **In-App**: Real-time notifications in application
- **Push Notifications**: Mobile/desktop push notifications
- **SMS**: Critical notifications (optional, configurable)

### Notification Preferences
- Users can configure which notifications they want to receive
- Channel preferences (email, in-app, push, SMS)
- Frequency preferences (instant, daily digest, weekly digest)
- Do Not Disturb schedule

---

## 🏗️ Domain Model

### 1. Notification Entity (Aggregate Root)

```csharp
public sealed class Notification : AggregateRoot<Guid>
{
    // Identity
    public Guid RecipientUserId { get; private set; }     // Who receives the notification
    public string RecipientEmail { get; private set; }    // Email address
    public string? RecipientPhone { get; private set; }   // Phone number (for SMS)

    // Notification Content
    public NotificationType Type { get; private set; }    // TaskAssigned, InvitationSent, etc.
    public NotificationCategory Category { get; private set; } // Task, User, Admin, System
    public string Title { get; private set; }             // Short title (max 200)
    public string Message { get; private set; }           // Full message (max 2000)
    public string? ActionUrl { get; private set; }        // Deep link to relevant page
    public string? ActionText { get; private set; }       // "View Task", "Accept Invitation"

    // Context
    public Guid? RelatedEntityId { get; private set; }    // Task ID, Invitation ID, etc.
    public string? RelatedEntityType { get; private set; } // "Task", "Invitation", "User"
    public Dictionary<string, string> Metadata { get; private set; } // Additional data

    // Delivery
    public List<NotificationChannel> Channels { get; private set; } // Email, InApp, Push, SMS
    public NotificationPriority Priority { get; private set; } // Low, Normal, High, Urgent
    public DateTime? ScheduledFor { get; private set; }   // For scheduled notifications
    public DateTime? SentAt { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Status Tracking
    public NotificationStatus Status { get; private set; } // Pending, Sent, Failed, Read
    public List<NotificationDelivery> Deliveries { get; private set; } // Track per channel

    // Retry Logic
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? FailureReason { get; private set; }

    // Factory Methods
    public static Notification CreateTaskAssignedNotification(
        Guid recipientUserId,
        string recipientEmail,
        Guid taskId,
        string taskTitle,
        string assignedBy)
    {
        var notification = new Notification(Guid.NewGuid())
        {
            RecipientUserId = recipientUserId,
            RecipientEmail = recipientEmail,
            Type = NotificationType.TaskAssigned,
            Category = NotificationCategory.Task,
            Title = "New Task Assigned",
            Message = $"You have been assigned to task: {taskTitle} by {assignedBy}",
            ActionUrl = $"/tasks/{taskId}",
            ActionText = "View Task",
            RelatedEntityId = taskId,
            RelatedEntityType = "Task",
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Email,
                NotificationChannel.InApp,
                NotificationChannel.Push
            },
            Priority = NotificationPriority.Normal,
            Status = NotificationStatus.Pending,
            Deliveries = new List<NotificationDelivery>(),
            RetryCount = 0,
            MaxRetries = 3,
            Metadata = new Dictionary<string, string>
            {
                ["TaskId"] = taskId.ToString(),
                ["TaskTitle"] = taskTitle,
                ["AssignedBy"] = assignedBy
            }
        };

        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent(notification.Id));
        return notification;
    }

    public static Notification CreateInvitationSentNotification(
        string recipientEmail,
        Guid invitationId,
        string masterUserName,
        bool recipientExists)
    {
        var title = recipientExists
            ? "New SubUser Invitation"
            : "You've been invited to TaskFlow";

        var message = recipientExists
            ? $"{masterUserName} has invited you to become their SubUser. You can now access their tasks."
            : $"{masterUserName} has invited you to join TaskFlow as their SubUser. Please register to accept the invitation.";

        var notification = new Notification(Guid.NewGuid())
        {
            RecipientUserId = Guid.Empty, // May not exist yet
            RecipientEmail = recipientEmail,
            Type = NotificationType.InvitationSent,
            Category = NotificationCategory.User,
            Title = title,
            Message = message,
            ActionUrl = $"/invitations/{invitationId}",
            ActionText = recipientExists ? "View Invitation" : "Register & Accept",
            RelatedEntityId = invitationId,
            RelatedEntityType = "Invitation",
            Channels = new List<NotificationChannel> { NotificationChannel.Email },
            Priority = NotificationPriority.Normal,
            Status = NotificationStatus.Pending,
            Deliveries = new List<NotificationDelivery>(),
            RetryCount = 0,
            MaxRetries = 3,
            Metadata = new Dictionary<string, string>
            {
                ["InvitationId"] = invitationId.ToString(),
                ["MasterUserName"] = masterUserName,
                ["RecipientExists"] = recipientExists.ToString()
            }
        };

        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent(notification.Id));
        return notification;
    }

    public static Notification CreateTaskCommentNotification(
        Guid recipientUserId,
        string recipientEmail,
        Guid taskId,
        string taskTitle,
        string commenterName,
        string commentPreview)
    {
        var notification = new Notification(Guid.NewGuid())
        {
            RecipientUserId = recipientUserId,
            RecipientEmail = recipientEmail,
            Type = NotificationType.TaskCommented,
            Category = NotificationCategory.Task,
            Title = "New Comment on Task",
            Message = $"{commenterName} commented on {taskTitle}: {commentPreview}",
            ActionUrl = $"/tasks/{taskId}#comments",
            ActionText = "View Comment",
            RelatedEntityId = taskId,
            RelatedEntityType = "Task",
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Email,
                NotificationChannel.InApp,
                NotificationChannel.Push
            },
            Priority = NotificationPriority.Normal,
            Status = NotificationStatus.Pending,
            Deliveries = new List<NotificationDelivery>(),
            RetryCount = 0,
            MaxRetries = 3,
            Metadata = new Dictionary<string, string>
            {
                ["TaskId"] = taskId.ToString(),
                ["TaskTitle"] = taskTitle,
                ["CommenterName"] = commenterName
            }
        };

        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent(notification.Id));
        return notification;
    }

    // Domain Methods
    public void MarkAsSent(NotificationChannel channel, string? externalId = null)
    {
        var delivery = NotificationDelivery.CreateSuccess(
            Id, channel, externalId);
        Deliveries.Add(delivery);

        if (Deliveries.Count(d => d.DeliveryStatus == DeliveryStatus.Sent) == Channels.Count)
        {
            Status = NotificationStatus.Sent;
            SentAt = DateTime.UtcNow;
            RaiseDomainEvent(new NotificationSentDomainEvent(Id, RecipientUserId));
        }
    }

    public void MarkAsFailed(NotificationChannel channel, string reason)
    {
        var delivery = NotificationDelivery.CreateFailure(
            Id, channel, reason);
        Deliveries.Add(delivery);

        RetryCount++;
        FailureReason = reason;

        if (RetryCount < MaxRetries)
        {
            // Exponential backoff: 5min, 15min, 45min
            var delayMinutes = Math.Pow(3, RetryCount) * 5;
            NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
            Status = NotificationStatus.Pending;
        }
        else
        {
            Status = NotificationStatus.Failed;
            RaiseDomainEvent(new NotificationFailedDomainEvent(Id, reason));
        }
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            RaiseDomainEvent(new NotificationReadDomainEvent(Id, RecipientUserId));
        }
    }

    public void Schedule(DateTime scheduledFor)
    {
        if (scheduledFor <= DateTime.UtcNow)
            throw new NotificationInvalidOperationException("Scheduled time must be in the future");

        ScheduledFor = scheduledFor;
        Status = NotificationStatus.Scheduled;
    }

    public bool ShouldRetry()
    {
        return Status == NotificationStatus.Pending
            && RetryCount < MaxRetries
            && NextRetryAt.HasValue
            && NextRetryAt.Value <= DateTime.UtcNow;
    }
}
```

### 2. NotificationDelivery Entity

```csharp
public sealed class NotificationDelivery : Entity<Guid>
{
    public Guid NotificationId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public DeliveryStatus DeliveryStatus { get; private set; }
    public DateTime AttemptedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? ExternalId { get; private set; }        // Email ID, Push token ID, etc.
    public string? FailureReason { get; private set; }
    public Dictionary<string, string> DeliveryMetadata { get; private set; }

    public static NotificationDelivery CreateSuccess(
        Guid notificationId,
        NotificationChannel channel,
        string? externalId)
    {
        return new NotificationDelivery(Guid.NewGuid())
        {
            NotificationId = notificationId,
            Channel = channel,
            DeliveryStatus = DeliveryStatus.Sent,
            AttemptedAt = DateTime.UtcNow,
            DeliveredAt = DateTime.UtcNow,
            ExternalId = externalId,
            DeliveryMetadata = new Dictionary<string, string>()
        };
    }

    public static NotificationDelivery CreateFailure(
        Guid notificationId,
        NotificationChannel channel,
        string reason)
    {
        return new NotificationDelivery(Guid.NewGuid())
        {
            NotificationId = notificationId,
            Channel = channel,
            DeliveryStatus = DeliveryStatus.Failed,
            AttemptedAt = DateTime.UtcNow,
            FailureReason = reason,
            DeliveryMetadata = new Dictionary<string, string>()
        };
    }
}
```

### 3. NotificationPreference Entity

```csharp
public sealed class NotificationPreference : Entity<Guid>
{
    public Guid UserId { get; private set; }

    // Channel Preferences
    public bool EmailEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool SmsEnabled { get; private set; }

    // Category Preferences (which categories to receive)
    public bool TaskNotificationsEnabled { get; private set; }
    public bool UserNotificationsEnabled { get; private set; }
    public bool AdminNotificationsEnabled { get; private set; }
    public bool SystemNotificationsEnabled { get; private set; }

    // Type-Specific Preferences
    public bool TaskAssignedEnabled { get; private set; }
    public bool TaskStatusChangedEnabled { get; private set; }
    public bool TaskCommentedEnabled { get; private set; }
    public bool TaskReviewRequestedEnabled { get; private set; }
    public bool InvitationSentEnabled { get; private set; }
    public bool InvitationAcceptedEnabled { get; private set; }

    // Frequency
    public NotificationFrequency Frequency { get; private set; } // Instant, DailyDigest, WeeklyDigest

    // Do Not Disturb
    public bool DoNotDisturbEnabled { get; private set; }
    public TimeSpan? DoNotDisturbStartTime { get; private set; } // e.g., 22:00
    public TimeSpan? DoNotDisturbEndTime { get; private set; }   // e.g., 08:00

    public static NotificationPreference CreateDefault(Guid userId)
    {
        return new NotificationPreference(Guid.NewGuid())
        {
            UserId = userId,
            EmailEnabled = true,
            InAppEnabled = true,
            PushEnabled = true,
            SmsEnabled = false,
            TaskNotificationsEnabled = true,
            UserNotificationsEnabled = true,
            AdminNotificationsEnabled = true,
            SystemNotificationsEnabled = true,
            TaskAssignedEnabled = true,
            TaskStatusChangedEnabled = true,
            TaskCommentedEnabled = true,
            TaskReviewRequestedEnabled = true,
            InvitationSentEnabled = true,
            InvitationAcceptedEnabled = true,
            Frequency = NotificationFrequency.Instant,
            DoNotDisturbEnabled = false
        };
    }

    public void UpdateChannelPreferences(
        bool email, bool inApp, bool push, bool sms)
    {
        EmailEnabled = email;
        InAppEnabled = inApp;
        PushEnabled = push;
        SmsEnabled = sms;
    }

    public void UpdateCategoryPreferences(
        bool task, bool user, bool admin, bool system)
    {
        TaskNotificationsEnabled = task;
        UserNotificationsEnabled = user;
        AdminNotificationsEnabled = admin;
        SystemNotificationsEnabled = system;
    }

    public void SetDoNotDisturb(TimeSpan startTime, TimeSpan endTime)
    {
        DoNotDisturbEnabled = true;
        DoNotDisturbStartTime = startTime;
        DoNotDisturbEndTime = endTime;
    }

    public void DisableDoNotDisturb()
    {
        DoNotDisturbEnabled = false;
        DoNotDisturbStartTime = null;
        DoNotDisturbEndTime = null;
    }

    public bool IsInDoNotDisturbPeriod()
    {
        if (!DoNotDisturbEnabled || !DoNotDisturbStartTime.HasValue || !DoNotDisturbEndTime.HasValue)
            return false;

        var now = DateTime.UtcNow.TimeOfDay;
        var start = DoNotDisturbStartTime.Value;
        var end = DoNotDisturbEndTime.Value;

        // Handle overnight DND (e.g., 22:00 - 08:00)
        if (start > end)
            return now >= start || now <= end;

        return now >= start && now <= end;
    }

    public List<NotificationChannel> GetEnabledChannels()
    {
        var channels = new List<NotificationChannel>();
        if (EmailEnabled) channels.Add(NotificationChannel.Email);
        if (InAppEnabled) channels.Add(NotificationChannel.InApp);
        if (PushEnabled) channels.Add(NotificationChannel.Push);
        if (SmsEnabled) channels.Add(NotificationChannel.SMS);
        return channels;
    }
}
```

### 4. NotificationTemplate Entity

```csharp
public sealed class NotificationTemplate : Entity<Guid>
{
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; }            // For email
    public string BodyTemplate { get; private set; }       // HTML/Plain text with placeholders
    public string? SmsTemplate { get; private set; }       // Short SMS template
    public Dictionary<string, string> DefaultPlaceholders { get; private set; }
    public bool IsActive { get; private set; }

    public string Render(Dictionary<string, string> data)
    {
        var result = BodyTemplate;
        foreach (var kvp in data)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return result;
    }
}
```

---

## 🔢 Enumerations

### NotificationType
```csharp
public enum NotificationType
{
    // Task Notifications
    TaskCreated = 1,
    TaskAssigned = 2,
    TaskReassigned = 3,
    TaskStatusChanged = 4,
    TaskCommented = 5,
    TaskReviewRequested = 6,
    TaskReviewCompleted = 7,
    TaskDismissed = 8,
    TaskSuspended = 9,
    TaskDeadlineApproaching = 10,
    TaskOverdue = 11,

    // User/Invitation Notifications
    UserRegistered = 20,
    EmailConfirmationRequired = 21,
    EmailConfirmed = 22,
    InvitationSent = 23,
    InvitationReceived = 24,
    InvitationAccepted = 25,
    InvitationRejected = 26,
    InvitationExpired = 27,
    UserPromotedToAdmin = 28,
    UserDemotedFromAdmin = 29,

    // Admin Action Notifications
    UserBlocked = 30,
    UserUnblocked = 31,
    UserSuspended = 32,
    UserActivated = 33,
    AccountLocked = 34,

    // System Notifications
    PasswordResetRequested = 40,
    PasswordChanged = 41,
    TwoFactorEnabled = 42,
    SecurityAlert = 43,
    WelcomeMessage = 44
}
```

### NotificationCategory
```csharp
public enum NotificationCategory
{
    Task = 1,
    User = 2,
    Admin = 3,
    System = 4
}
```

### NotificationChannel
```csharp
public enum NotificationChannel
{
    Email = 1,
    InApp = 2,
    Push = 3,
    SMS = 4
}
```

### NotificationPriority
```csharp
public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}
```

### NotificationStatus
```csharp
public enum NotificationStatus
{
    Pending = 1,
    Scheduled = 2,
    Sent = 3,
    Failed = 4,
    Read = 5
}
```

### DeliveryStatus
```csharp
public enum DeliveryStatus
{
    Sent = 1,
    Failed = 2
}
```

### NotificationFrequency
```csharp
public enum NotificationFrequency
{
    Instant = 1,
    DailyDigest = 2,
    WeeklyDigest = 3
}
```

---

## 📊 DTOs

### NotificationDto
```csharp
public sealed record NotificationDto(
    Guid Id,
    Guid RecipientUserId,
    NotificationType Type,
    NotificationCategory Category,
    string Title,
    string Message,
    string? ActionUrl,
    string? ActionText,
    NotificationPriority Priority,
    bool IsRead,
    DateTime? ReadAt,
    NotificationStatus Status,
    DateTime CreatedAt
);
```

### CreateNotificationRequest
```csharp
public sealed record CreateNotificationRequest(
    Guid RecipientUserId,
    NotificationType Type,
    string Title,
    string Message,
    string? ActionUrl = null,
    string? ActionText = null,
    Guid? RelatedEntityId = null,
    NotificationPriority Priority = NotificationPriority.Normal
);
```

### NotificationPreferenceDto
```csharp
public sealed record NotificationPreferenceDto(
    Guid UserId,
    bool EmailEnabled,
    bool InAppEnabled,
    bool PushEnabled,
    bool SmsEnabled,
    bool TaskNotificationsEnabled,
    bool UserNotificationsEnabled,
    bool AdminNotificationsEnabled,
    bool SystemNotificationsEnabled,
    NotificationFrequency Frequency,
    bool DoNotDisturbEnabled
);
```

---

### Validators

**FluentValidation validators for all commands**

```csharp
public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.RecipientUserId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
```

---

## 💼 Application Layer

### Commands

#### 1. SendNotificationCommand
```csharp
public sealed record SendNotificationCommand(
    Guid RecipientUserId,
    NotificationType Type,
    string Title,
    string Message,
    string? ActionUrl,
    Guid? RelatedEntityId
) : IRequest<Result<Guid>>;

// Handler creates notification and triggers delivery
```

#### 2. MarkNotificationAsReadCommand
```csharp
public sealed record MarkNotificationAsReadCommand(
    Guid NotificationId,
    Guid UserId
) : IRequest<Result>;
```

#### 3. MarkAllAsReadCommand
```csharp
public sealed record MarkAllAsReadCommand(
    Guid UserId
) : IRequest<Result>;
```

#### 4. UpdateNotificationPreferencesCommand
```csharp
public sealed record UpdateNotificationPreferencesCommand(
    Guid UserId,
    bool EmailEnabled,
    bool InAppEnabled,
    bool PushEnabled,
    bool SmsEnabled,
    Dictionary<NotificationType, bool> TypePreferences
) : IRequest<Result>;
```

#### 5. SetDoNotDisturbCommand
```csharp
public sealed record SetDoNotDisturbCommand(
    Guid UserId,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<Result>;
```

### Queries

#### 1. GetUserNotificationsQuery
```csharp
public sealed record GetUserNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<NotificationDto>>>;
```

#### 2. GetUnreadCountQuery
```csharp
public sealed record GetUnreadCountQuery(
    Guid UserId
) : IRequest<Result<int>>;
```

#### 3. GetNotificationPreferencesQuery
```csharp
public sealed record GetNotificationPreferencesQuery(
    Guid UserId
) : IRequest<Result<NotificationPreferenceDto>>;
```

---

## 🔒 Business Rules

### Notification Creation Rules
1. Notification must have recipient (UserId or Email)
2. Title required (max 200 characters)
3. Message required (max 2000 characters)
4. Type and Category must be valid enums
5. Priority defaults to Normal if not specified
6. Channels determined by user preferences (if user exists)
7. For new users (email only), default to Email channel
8. RelatedEntityId tracked for context
9. Metadata stored for template rendering
10. MaxRetries defaults to 3

### Delivery Rules
11. Check user preferences before sending
12. Respect Do Not Disturb schedule (queue for later)
13. If DND active, defer to next available window
14. Send to all enabled channels in parallel
15. Track delivery status per channel
16. Mark as Sent when ALL channels succeed
17. Mark as Failed if ALL channels fail after max retries
18. Exponential backoff for retries (5min, 15min, 45min)
19. Store external IDs (email ID, push token) for tracking
20. InApp notifications always delivered (no retry logic)

### Read Status Rules
21. MarkAsRead updates IsRead and ReadAt
22. Only recipient can mark as read
23. Read status tracked per notification (not per channel)
24. Unread count updated real-time
25. Read notifications remain in history

### Preference Rules
26. Default preferences created on user registration
27. Users can disable any channel
28. Users can disable specific notification types
29. Users can disable entire categories
30. Frequency: Instant (default), Daily Digest, Weekly Digest
31. Digest notifications batched and sent at configured time
32. DND schedule: Start time < End time OR overnight (22:00-08:00)
33. Urgent notifications bypass DND
34. Admin notifications always enabled (cannot disable)

### Task Notification Triggers
35. TaskAssigned: Notify assignee (Email + InApp + Push)
36. TaskReassigned: Notify new assignee and old assignee
37. TaskStatusChanged: Notify creator and assignee
38. TaskCommented: Notify creator, assignee, and previous commenters
39. TaskReviewRequested: Notify creator (High priority)
40. TaskReviewCompleted: Notify assignee
41. TaskDismissed: Notify creator and assignee (High priority)
42. TaskSuspended: Notify creator and assignee
43. TaskDeadlineApproaching: Notify assignee (24h, 1h before)
44. TaskOverdue: Notify assignee and creator (Urgent priority)

### Invitation Notification Triggers
45. InvitationSent: Notify recipient via Email
46. If recipient exists: Include deep link to accept/reject
47. If recipient doesn't exist: Include registration link
48. InvitationAccepted: Notify master user
49. InvitationRejected: Notify master user
50. InvitationExpired: Notify master user (after 7 days)

### Admin Action Notification Triggers
51. UserBlocked: Notify user via Email (High priority)
52. UserSuspended: Notify user with reason and duration
53. UserPromotedToAdmin: Notify user (High priority)
54. UserDemotedFromAdmin: Notify user

### System Notification Triggers
55. UserRegistered: Send welcome email with confirmation link
56. EmailConfirmationRequired: Send confirmation link
57. PasswordResetRequested: Send reset link (Urgent, Email only, 1-hour expiry)
58. PasswordChanged: Send confirmation (Security alert)
59. AccountLocked: Notify user of lockout (Urgent)
60. SecurityAlert: New device/location login (High priority)

### Template Rules
61. Templates stored per Type + Channel combination
62. Email templates use HTML with fallback plain text
63. SMS templates max 160 characters
64. Push notifications max 256 characters (title + message)
65. InApp notifications support rich HTML
66. Placeholders: {{UserName}}, {{TaskTitle}}, {{ActionUrl}}, etc.
67. Templates support localization (future)

---

## 🎬 User Flows

### Flow 1: Task Assigned Notification

**Scenario**: User A assigns a task to User B

**Steps**:
1. User A assigns Task #123 to User B via AssignTaskCommand
2. Command handler raises TaskAssignedDomainEvent
3. TaskAssignedEventHandler listens to event
4. Handler calls SendNotificationCommand
5. SendNotificationCommandHandler:
   - Creates Notification entity with Type=TaskAssigned
   - Loads User B's NotificationPreferences
   - Checks TaskAssignedEnabled = true
   - Checks GetEnabledChannels() → [Email, InApp, Push]
   - Checks IsInDoNotDisturbPeriod() → false
   - Sets Channels based on preferences
   - Persists Notification
6. NotificationProcessor (background service):
   - Picks up pending notification
   - Sends via EmailService → Success → ExternalId stored
   - Sends via InAppService → Success (SignalR)
   - Sends via PushService → Success
   - Marks delivery as Sent for each channel
   - Updates Notification.Status = Sent
7. User B:
   - Receives email with "View Task" button
   - Sees in-app notification badge (unread count: 1)
   - Gets push notification on mobile
   - Clicks "View Task" → Redirected to task page
   - Notification marked as Read

**Result**:
- User B notified across all preferred channels
- Notification tracked in database
- Unread count updated real-time

---

### Flow 2: SubUser Invitation (Existing User)

**Scenario**: Master User invites existing user as SubUser

**Steps**:
1. Master User sends invitation to existing@email.com
2. SendInvitationCommand handler:
   - Creates UserMasterSubUserRelation
   - Checks if existing@email.com exists → YES
   - Loads recipient's UserId
   - Raises InvitationSentDomainEvent
3. InvitationSentEventHandler:
   - Calls CreateInvitationSentNotification()
   - recipientExists = true
   - Title: "New SubUser Invitation"
   - Message: "Master User has invited you..."
   - ActionUrl: "/invitations/{id}"
   - ActionText: "View Invitation"
   - Channel: Email (always for invitations)
4. NotificationProcessor:
   - Sends email with invitation details
   - Email includes "Accept" and "Reject" buttons
   - Marks as Sent
5. Recipient:
   - Receives email
   - Clicks "Accept"
   - Redirected to app (authenticated)
   - AcceptInvitationCommand executed
6. InvitationAcceptedEventHandler:
   - Sends notification to Master User
   - Type: InvitationAccepted
   - Message: "existing@email.com accepted your invitation"

**Result**:
- Recipient notified via email
- Master notified when accepted
- Complete audit trail

---

### Flow 3: SubUser Invitation (New User)

**Scenario**: Master User invites non-existent user

**Steps**:
1. Master User sends invitation to newuser@email.com
2. SendInvitationCommand handler:
   - Creates UserMasterSubUserRelation with SubUserId = null
   - Checks if newuser@email.com exists → NO
   - Raises InvitationSentDomainEvent
3. InvitationSentEventHandler:
   - Calls CreateInvitationSentNotification()
   - recipientExists = false
   - Title: "You've been invited to TaskFlow"
   - Message: "Master User has invited you... Please register first"
   - ActionUrl: "/register?invitation={id}"
   - ActionText: "Register & Accept"
   - Channel: Email only
4. NotificationProcessor:
   - Sends email to newuser@email.com
   - Email includes registration link with invitation token
5. Recipient:
   - Receives email
   - Clicks "Register & Accept"
   - Completes registration
   - Completes profile (two-stage)
6. After profile completion:
   - System auto-matches pending invitation by email
   - Updates UserMasterSubUserRelation.SubUserId
   - Sends InvitationAccepted notification to Master User

**Result**:
- New user onboarded via invitation
- Invitation auto-accepted after profile completion
- Master notified

---

### Flow 4: Task Commented with Multiple Recipients

**Scenario**: User C comments on a task, notify creator and assignee

**Steps**:
1. User C adds comment to Task #123 via AddCommentCommand
2. Command handler raises TaskCommentedDomainEvent
3. TaskCommentedEventHandler:
   - Loads Task #123
   - Identifies recipients:
     - Creator (User A)
     - Current Assignee (User B)
     - Previous Commenters (User D, User E)
   - For each recipient:
     - Check if recipient != commenter (User C)
     - Check NotificationPreferences.TaskCommentedEnabled
     - Create notification
4. NotificationProcessor processes all notifications
5. Recipients see notifications based on their preferences

**Result**:
- All relevant users notified
- No notification to commenter (self)
- Respects individual preferences

---

### Flow 5: Admin Blocks User

**Scenario**: Admin blocks User X

**Steps**:
1. Admin executes BlockUserCommand for User X
2. Command handler:
   - Updates AppUser.Status = Locked
   - Revokes all refresh tokens
   - Raises UserBlockedDomainEvent
3. UserBlockedEventHandler:
   - Creates notification for User X
   - Type: UserBlocked
   - Priority: High
   - Message: "Your account has been blocked by admin. Reason: [reason]"
   - ActionText: "Contact Support"
   - Channels: Email (force, ignore preferences)
4. NotificationProcessor:
   - Sends urgent email to user
   - Includes reason and support contact

**Result**:
- User notified immediately
- Critical notification delivered regardless of preferences

---

## 🗄️ Infrastructure Layer

### Repository Interface

```csharp
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync(
        int batchSize = 100,
        CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetScheduledNotificationsAsync(
        DateTime now,
        CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    void Update(Notification notification);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(NotificationPreference preference, CancellationToken ct = default);
    void Update(NotificationPreference preference);
}
```

### EF Core Configuration

```csharp
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientUserId).IsRequired();
        builder.HasIndex(x => x.RecipientUserId);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);

        builder.Property(x => x.Type).IsRequired().HasConversion<string>();
        builder.Property(x => x.Category).IsRequired().HasConversion<string>();
        builder.Property(x => x.Priority).IsRequired().HasConversion<string>();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();

        builder.OwnsMany(x => x.Deliveries, d =>
        {
            d.ToTable("NotificationDeliveries");
            d.WithOwner().HasForeignKey("NotificationId");
            d.Property(x => x.Channel).IsRequired().HasConversion<string>();
            d.Property(x => x.DeliveryStatus).IsRequired().HasConversion<string>();
        });

        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null));
    }
}
```

### External Services

```csharp
public interface IEmailService
{
    Task<Result<string>> SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken ct = default);
}

public interface ISmsService
{
    Task<Result<string>> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default);
}

public interface IPushNotificationService
{
    Task<Result<string>> SendPushNotificationAsync(
        Guid userId,
        string title,
        string message,
        string? actionUrl,
        CancellationToken ct = default);
}

public interface IInAppNotificationService
{
    Task SendInAppNotificationAsync(
        Guid userId,
        NotificationDto notification,
        CancellationToken ct = default);
}
```

### Background Services

```csharp
// Process pending notifications
public sealed class NotificationProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var notifications = await _repository.GetPendingNotificationsAsync(100, ct);

            foreach (var notification in notifications)
            {
                if (notification.ShouldRetry())
                {
                    await ProcessNotificationAsync(notification, ct);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }
    }

    private async Task ProcessNotificationAsync(Notification notification, CancellationToken ct)
    {
        foreach (var channel in notification.Channels)
        {
            try
            {
                var externalId = await SendViaChannelAsync(channel, notification, ct);
                notification.MarkAsSent(channel, externalId);
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed(channel, ex.Message);
            }
        }

        await _repository.SaveChangesAsync(ct);
    }
}

// Process scheduled notifications
public sealed class ScheduledNotificationProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var notifications = await _repository.GetScheduledNotificationsAsync(now, ct);

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Pending;
            }

            await _repository.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}
```

---

## 🌐 API Layer

### NotificationsController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var query = new GetUserNotificationsQuery(userId, unreadOnly, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new MarkNotificationAsReadCommand(id, userId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var command = new MarkAllAsReadCommand(userId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetCurrentUserId();
        var query = new GetNotificationPreferencesQuery(userId);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request)
    {
        var userId = GetCurrentUserId();
        var command = request.Adapt<UpdateNotificationPreferencesCommand>();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
```

---

### Configuration

**appsettings.json**:
```json
{
  "EmailSettings": {
    "ApiKey": "your-sendgrid-api-key",
    "FromName": "TaskFlow",
    "FromEmail": "noreply@taskflow.com"
  },
  "SmsSettings": {
    "AccountSid": "your-twilio-account-sid",
    "AuthToken": "your-twilio-auth-token",
    "FromPhoneNumber": "+1234567890"
  },
  "PushNotificationSettings": {
    "ServiceType": "Firebase", // or "APNS"
    "ServerKey": "your-firebase-server-key"
  },
  "NotificationProcessor": {
    "BatchSize": 100,
    "PollingIntervalSeconds": 10
  }
}
```

---

## 🎯 Domain Events

```csharp
public sealed record NotificationCreatedDomainEvent(Guid NotificationId) : IDomainEvent;
public sealed record NotificationSentDomainEvent(Guid NotificationId, Guid RecipientUserId) : IDomainEvent;
public sealed record NotificationFailedDomainEvent(Guid NotificationId, string Reason) : IDomainEvent;
public sealed record NotificationReadDomainEvent(Guid NotificationId, Guid UserId) : IDomainEvent;
```

---

## 🚨 Domain Exceptions

```csharp
public sealed class NotificationNotFoundException : NotFoundException
{
    public NotificationNotFoundException(Guid notificationId)
        : base($"Notification with ID {notificationId} was not found") { }
}

public sealed class NotificationInvalidOperationException : DomainException
{
    public NotificationInvalidOperationException(string message)
        : base(message) { }
}

public sealed class NotificationDeliveryException : DomainException
{
    public NotificationDeliveryException(string channel, string reason)
        : base($"Failed to deliver via {channel}: {reason}") { }
}
```

---

## 📝 Implementation Notes

### Integration with Other Services
1. **Task Service**: Publishes integration events (TaskAssignedEvent, TaskCommentedEvent, etc.)
2. **User Service**: Publishes integration events (InvitationSentEvent, UserPromotedEvent, etc.)
3. **Notification Service**: Subscribes to all events and creates notifications
4. **Event-Driven Architecture**: Uses RabbitMQ/Service Bus for decoupled communication

### Real-Time Notifications (SignalR)
1. InApp channel uses SignalR for real-time push
2. Users connected via WebSocket
3. Hub method: `SendNotificationToUser(userId, notification)`
4. Client receives and updates UI immediately
5. Unread count badge updates in real-time

### Email Templates
1. Store HTML templates with Liquid/Razor syntax
2. Template variables: {{UserName}}, {{ActionUrl}}, {{TaskTitle}}, etc.
3. Support for multiple languages (future)
4. Responsive HTML for mobile devices
5. Plain text fallback always included

### Performance Considerations
1. Batch processing: Process 100 notifications at a time
2. Parallel delivery: Send to all channels concurrently
3. Caching: Cache user preferences (5-minute TTL)
4. Indexing: Index on (RecipientUserId, IsRead, CreatedAt)
5. Archiving: Move read notifications older than 90 days to archive table

### Security
1. Users can only read their own notifications
2. Users can only update their own preferences
3. Admin notifications always delivered (override preferences)
4. Validate recipient exists before creating notification
5. Rate limiting: Max 100 notifications per user per hour

## 🎉 Summary

---

## 🚀 Implementation Steps

1.  **✅ Domain Layer (COMPLETED)**
    *   Notification entity
    *   NotificationDelivery entity
    *   NotificationPreference entity
    *   NotificationTemplate entity
    *   Domain events
    *   Enums

2.  **📋 Application Layer (NEXT)**
    *   DTOs
    *   Commands + Handlers + Validators
    *   Queries + Handlers
    *   Repository interfaces

3.  **🔧 Infrastructure Layer**
    *   Email, SMS, Push notification services
    *   Repository implementations
    *   EF Core configurations
    *   Background services for processing notifications

4.  **🌐 API Layer**
    *   Notifications controller
    *   SignalR Hub for real-time notifications

5.  **🧪 Testing**
    *   Unit tests (Domain, Application)
    *   Integration tests (API)

---

## 🔒 Security Considerations

-   **Authorization**: Users must only be able to access their own notifications and preferences. API endpoints must be protected with `[Authorize]` and user ID claims must be validated against the resource being accessed.
-   **Data Validation**: All input from users, especially in API requests, must be rigorously validated to prevent injection attacks (e.g., XSS in notification messages).
-   **Rate Limiting**: Implement rate limiting on notification creation to prevent abuse and spam.
-   **Secure URLs**: Any links in notifications (e.g., `ActionUrl`) must be validated to prevent open redirect vulnerabilities.
-   **Sensitive Data**: Avoid sending sensitive information in notifications. For example, instead of sending a new password, send a link to reset it.
-   **External Services**: API keys and secrets for external services (email, SMS) must be stored securely using the secret manager or Azure Key Vault, not in `appsettings.json`.

---


