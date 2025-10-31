# Task Feature - Complete Specification

## Overview

Production-ready Task management system for TaskFlow microservices platform with **assignment tracking**, **reassignment capabilities**, **review system**, **multi-user comments with attachments**, and **comprehensive activity history**.

**Key Principles**:
- ✅ Any user (with completed profile) can create tasks
- ✅ Task can be assigned to **ONE user at a time** (single assignee)
- ✅ Assignee can update task status and progress
- ✅ Assignee can **reassign** task (if creator allows it)
- ✅ Assignee can mark task as complete with review and star rating
- ✅ **Multiple users can comment** on task (creator, assignee, others if shared)
- ✅ Comments support **attachments** (images, videos, documents)
- ✅ **Complete audit trail**: Every status change, assignment, reassignment tracked
- ✅ **Time tracking**: Hours taken per status, total time
- ✅ **Complexity levels**: Low, Medium, High, Critical
- ✅ **Master-SubUser integration**: SubUsers can view/manage MasterUser's tasks based on permissions

---

## Architecture

### Domain Model

#### Core Concepts

**1. Task Lifecycle**:
- **Created** → Task created by user
- **Assigned** → Task assigned to specific user
- **InProgress** → Assignee starts working
- **UnderReview** → Assignee marks complete, awaiting creator review
- **Completed** → Creator approves with review/rating
- **Reassigned** → Task reassigned to different user
- **Cancelled** → Task cancelled by creator

**2. Assignment Model**:
- Task has **ONE active assignee** at a time
- Assignment history tracked in **TaskAssignment** entity
- Reassignment allowed if `AllowReassignment = true`
- Assignee can reassign to creator or any other user

**3. Comment System**:
- Multiple users can comment
- Comments support attachments (images, videos, files)
- Attachments stored with metadata (filename, size, type, URL)
- Comments immutable (no edit/delete, only add)

**4. History & Audit Trail**:
- Every action tracked in **TaskHistory**
- Status changes, assignments, reassignments, time logs
- Who did what, when, and why (optional notes)

#### Entities

**Task** (Aggregate Root):
- Primary entity for task management
- Properties:
  - `Id` (Guid) - Primary key
  - `Title` (string, required, max 200) - Task title
  - `Description` (string, optional, max 5000) - Detailed description
  - `CreatedBy` (Guid) - Foreign key to UserEntity (creator)
  - `CreatedByUser` (UserEntity) - Navigation to creator
  - `AssignedTo` (Guid?) - Foreign key to UserEntity (current assignee)
  - `AssignedToUser` (UserEntity?) - Navigation to current assignee
  - `Status` (TaskStatus enum) - Current status
  - `Priority` (TaskPriority enum) - Low, Medium, High, Critical
  - `ComplexityLevel` (ComplexityLevel enum) - Low, Medium, High, Critical
  - `DueDate` (DateTime?) - Optional due date
  - `EstimatedHours` (decimal?) - Estimated time to complete
  - `ActualHours` (decimal?) - Actual time taken (calculated from history)
  - `AllowReassignment` (bool) - Can assignee reassign to others?
  - `RequiresReview` (bool) - Does task need creator review before completion?
  - `ReviewRating` (int?) - Star rating (1-5) given by creator
  - `ReviewComment` (string?, max 2000) - Review comment from creator
  - `ReviewedAt` (DateTime?) - When task was reviewed
  - `ReviewedBy` (Guid?) - Who reviewed (usually creator)
  - `CompletedAt` (DateTime?) - When task marked complete
  - `CancelledAt` (DateTime?) - When task was cancelled
  - `CancellationReason` (string?, max 1000) - Why task was cancelled
  - `Tags` (List<string>) - Tags for categorization
  - `Assignments` (List<TaskAssignment>) - Assignment history
  - `Comments` (List<TaskComment>) - All comments
  - `History` (List<TaskHistory>) - Complete audit trail
  - `CreatedAt` (DateTime) - Task creation time
  - `UpdatedAt` (DateTime?) - Last update time

**TaskAssignment** (Entity):
- Tracks assignment history
- Properties:
  - `Id` (Guid) - Primary key
  - `TaskId` (Guid) - Foreign key to Task
  - `Task` (Task) - Navigation to task
  - `AssignedTo` (Guid) - Foreign key to UserEntity (assignee)
  - `AssignedToUser` (UserEntity) - Navigation to assignee
  - `AssignedBy` (Guid) - Foreign key to UserEntity (who assigned)
  - `AssignedByUser` (UserEntity) - Navigation to assigner
  - `AssignmentType` (AssignmentType enum) - Initial, Reassignment
  - `AssignedAt` (DateTime) - When assigned
  - `UnassignedAt` (DateTime?) - When unassigned (reassigned)
  - `IsActive` (bool) - Currently active assignment
  - `Notes` (string?, max 500) - Optional assignment notes
  - `StartedAt` (DateTime?) - When assignee started work
  - `CompletedAt` (DateTime?) - When assignee marked complete

**TaskComment** (Entity):
- Comments on tasks
- Properties:
  - `Id` (Guid) - Primary key
  - `TaskId` (Guid) - Foreign key to Task
  - `Task` (Task) - Navigation to task
  - `UserId` (Guid) - Foreign key to UserEntity (commenter)
  - `User` (UserEntity) - Navigation to commenter
  - `Content` (string, required, max 5000) - Comment text
  - `Attachments` (List<CommentAttachment>) - Attachments
  - `ParentCommentId` (Guid?) - For threaded comments (reply)
  - `ParentComment` (TaskComment?) - Navigation to parent
  - `Replies` (List<TaskComment>) - Navigation to replies
  - `CreatedAt` (DateTime) - When comment posted
  - `IsEdited` (bool) - Was comment edited? (future feature)
  - `EditedAt` (DateTime?) - When edited (future feature)

**CommentAttachment** (Value Object - Owned by TaskComment):
- Attachments for comments
- Properties:
  - `Id` (Guid) - Primary key
  - `FileName` (string, required, max 255) - Original filename
  - `FileSize` (long) - File size in bytes
  - `FileType` (string, max 100) - MIME type (image/png, video/mp4, etc.)
  - `FileUrl` (string, required, max 1000) - Storage URL
  - `ThumbnailUrl` (string?, max 1000) - Thumbnail URL (for images/videos)
  - `UploadedAt` (DateTime) - Upload timestamp

**TaskHistory** (Entity):
- Complete audit trail
- Properties:
  - `Id` (Guid) - Primary key
  - `TaskId` (Guid) - Foreign key to Task
  - `Task` (Task) - Navigation to task
  - `Action` (TaskAction enum) - Created, Assigned, Reassigned, StatusChanged, Completed, etc.
  - `PerformedBy` (Guid) - Foreign key to UserEntity (who did action)
  - `PerformedByUser` (UserEntity) - Navigation to user
  - `PreviousValue` (string?, max 500) - Old value (JSON)
  - `NewValue` (string?, max 500) - New value (JSON)
  - `HoursTaken` (decimal?) - Hours spent in this stage
  - `Notes` (string?, max 1000) - Optional notes
  - `PerformedAt` (DateTime) - When action occurred

#### Enums

**TaskStatus**:
```csharp
public enum TaskStatus
{
    Draft = 1,           // Created but not assigned
    Assigned = 2,        // Assigned to user
    InProgress = 3,      // Assignee started work
    Blocked = 4,         // Blocked by dependency/issue
    UnderReview = 5,     // Assignee marked complete, awaiting review
    Completed = 6,       // Creator approved/reviewed
    Cancelled = 7        // Task cancelled
}
```

**TaskPriority**:
```csharp
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
```

**ComplexityLevel**:
```csharp
public enum ComplexityLevel
{
    Low = 1,       // 1-4 hours
    Medium = 2,    // 4-16 hours
    High = 3,      // 16-40 hours
    Critical = 4   // 40+ hours
}
```

**AssignmentType**:
```csharp
public enum AssignmentType
{
    Initial = 1,      // First assignment
    Reassignment = 2  // Reassigned from someone else
}
```

**TaskAction** (for history):
```csharp
public enum TaskAction
{
    Created = 1,
    Assigned = 2,
    Reassigned = 3,
    StatusChanged = 4,
    Started = 5,
    Blocked = 6,
    Unblocked = 7,
    MarkedComplete = 8,
    Reviewed = 9,
    Completed = 10,
    Cancelled = 11,
    CommentAdded = 12,
    AttachmentAdded = 13,
    DueDateChanged = 14,
    PriorityChanged = 15
}
```

---

## Application Layer

### DTOs (Data Transfer Objects)

**Task DTOs**:
```csharp
// Response DTOs
public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    Guid CreatedBy,
    string CreatedByName,
    Guid? AssignedTo,
    string? AssignedToName,
    TaskStatus Status,
    TaskPriority Priority,
    ComplexityLevel ComplexityLevel,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    bool AllowReassignment,
    bool RequiresReview,
    int? ReviewRating,
    string? ReviewComment,
    DateTime? ReviewedAt,
    DateTime? CompletedAt,
    List<string> Tags,
    int CommentsCount,
    int AttachmentsCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid CreatedBy,
    string CreatedByName,
    Guid? AssignedTo,
    string? AssignedToName,
    TaskStatus Status,
    TaskPriority Priority,
    ComplexityLevel ComplexityLevel,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    bool AllowReassignment,
    bool RequiresReview,
    int? ReviewRating,
    string? ReviewComment,
    DateTime? ReviewedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    List<string> Tags,
    List<TaskAssignmentDto> Assignments,
    List<TaskCommentDto> Comments,
    List<TaskHistoryDto> History,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Request DTOs
public record CreateTaskRequest(
    string Title,
    string? Description,
    Guid? AssignToUserId,  // Optional: assign immediately or leave as draft
    TaskPriority Priority,
    ComplexityLevel ComplexityLevel,
    DateTime? DueDate,
    decimal? EstimatedHours,
    bool AllowReassignment,
    bool RequiresReview,
    List<string> Tags);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    ComplexityLevel ComplexityLevel,
    DateTime? DueDate,
    decimal? EstimatedHours,
    List<string> Tags);

public record AssignTaskRequest(
    Guid TaskId,
    Guid AssignToUserId,
    string? Notes);

public record ReassignTaskRequest(
    Guid TaskId,
    Guid ReassignToUserId,
    string? Reason);

public record ChangeTaskStatusRequest(
    Guid TaskId,
    TaskStatus NewStatus,
    string? Notes);

public record MarkTaskCompleteRequest(
    Guid TaskId,
    decimal? HoursSpent,
    string? CompletionNotes);

public record ReviewTaskRequest(
    Guid TaskId,
    int Rating,  // 1-5 stars
    string? ReviewComment);

public record CancelTaskRequest(
    Guid TaskId,
    string Reason);
```

**Comment DTOs**:
```csharp
public record TaskCommentDto(
    Guid Id,
    Guid TaskId,
    Guid UserId,
    string UserName,
    string Content,
    List<CommentAttachmentDto> Attachments,
    Guid? ParentCommentId,
    int RepliesCount,
    DateTime CreatedAt);

public record CommentAttachmentDto(
    Guid Id,
    string FileName,
    long FileSize,
    string FileType,
    string FileUrl,
    string? ThumbnailUrl,
    DateTime UploadedAt);

public record AddTaskCommentRequest(
    Guid TaskId,
    string Content,
    Guid? ParentCommentId,  // For replies
    List<AttachmentUploadDto> Attachments);

public record AttachmentUploadDto(
    string FileName,
    string Base64Content,  // Or URL if uploaded separately
    string FileType);
```

**Assignment DTOs**:
```csharp
public record TaskAssignmentDto(
    Guid Id,
    Guid TaskId,
    Guid AssignedTo,
    string AssignedToName,
    Guid AssignedBy,
    string AssignedByName,
    AssignmentType AssignmentType,
    DateTime AssignedAt,
    DateTime? UnassignedAt,
    bool IsActive,
    string? Notes,
    DateTime? StartedAt,
    DateTime? CompletedAt);
```

**History DTOs**:
```csharp
public record TaskHistoryDto(
    Guid Id,
    Guid TaskId,
    TaskAction Action,
    Guid PerformedBy,
    string PerformedByName,
    string? PreviousValue,
    string? NewValue,
    decimal? HoursTaken,
    string? Notes,
    DateTime PerformedAt);
```

---

## Commands (Write Operations)

### Task Management Commands

1. **CreateTaskCommand**
   - Creates new task
   - Sets creator as current user
   - Optionally assigns immediately
   - Creates initial TaskHistory entry
   - Returns task DTO

2. **UpdateTaskCommand**
   - Updates task details (title, description, priority, etc.)
   - Only creator can update
   - Creates TaskHistory entry
   - Returns updated task

3. **AssignTaskCommand**
   - Assigns task to user
   - Creates TaskAssignment record
   - Updates Task.AssignedTo
   - Sets Status = Assigned
   - Creates TaskHistory entry
   - Sends notification to assignee
   - Returns task DTO

4. **ReassignTaskCommand** ⭐
   - **Validation**:
     - Assignee must be current assignee
     - AllowReassignment must be true (OR assignee is creator)
     - Target user must have completed profile
   - Marks current assignment as inactive
   - Creates new TaskAssignment record
   - Updates Task.AssignedTo
   - Creates TaskHistory entry (Action = Reassigned)
   - Sends notification to new assignee
   - Returns task DTO

5. **ChangeTaskStatusCommand**
   - Changes task status (InProgress, Blocked, etc.)
   - Only assignee or creator can change status
   - Validates status transitions
   - Creates TaskHistory entry with hours taken
   - Returns task DTO

6. **StartTaskCommand**
   - Assignee starts working
   - Status: Assigned → InProgress
   - Records StartedAt in assignment
   - Creates TaskHistory entry
   - Returns task DTO

7. **MarkTaskCompleteCommand** ⭐
   - Assignee marks task complete
   - If RequiresReview = true:
     - Status: InProgress → UnderReview
     - Sends notification to creator
   - If RequiresReview = false:
     - Status: InProgress → Completed
     - Sets CompletedAt
   - Records hours spent
   - Creates TaskHistory entry
   - Returns task DTO

8. **ReviewTaskCommand** ⭐
   - Creator reviews completed task
   - Sets rating (1-5 stars)
   - Adds review comment
   - Status: UnderReview → Completed
   - Sets CompletedAt, ReviewedAt
   - Creates TaskHistory entry
   - Returns task DTO

9. **CancelTaskCommand**
   - Creator cancels task
   - Status: Any → Cancelled
   - Sets CancelledAt, CancellationReason
   - Creates TaskHistory entry
   - Sends notification to assignee
   - Returns task DTO

### Comment Commands

10. **AddTaskCommentCommand**
    - Adds comment to task
    - Uploads and stores attachments
    - Creates CommentAttachment records
    - Creator, assignee, or authorized users can comment
    - Master-SubUser: SubUser can comment if CanViewTasks permission
    - Creates TaskHistory entry (Action = CommentAdded)
    - Sends notifications to relevant users
    - Returns comment DTO

11. **ReplyToCommentCommand**
    - Replies to existing comment (threaded)
    - Sets ParentCommentId
    - Same permissions as AddTaskCommentCommand
    - Returns comment DTO

---

## Queries (Read Operations)

### Task Queries

1. **GetMyTasksQuery**
   - Returns tasks where user is creator OR assignee
   - Supports filtering: Status, Priority, DueDate
   - Supports sorting: CreatedAt, DueDate, Priority
   - Pagination support
   - Returns List<TaskDto>

2. **GetTaskByIdQuery**
   - Returns complete task details
   - Includes assignments, comments, history
   - Authorization: Creator, assignee, or authorized users
   - Master-SubUser: SubUser can view if CanViewTasks permission
   - Returns TaskDetailDto

3. **GetCreatedTasksQuery**
   - Returns tasks created by current user
   - Pagination and filtering support
   - Returns List<TaskDto>

4. **GetAssignedTasksQuery**
   - Returns tasks assigned to current user
   - Pagination and filtering support
   - Returns List<TaskDto>

5. **GetTasksByUserQuery** (Admin)
   - Returns all tasks for specific user (created or assigned)
   - Admin only
   - Returns List<TaskDto>

6. **GetTaskHistoryQuery**
   - Returns complete history for task
   - Chronological order
   - Returns List<TaskHistoryDto>

7. **GetTaskCommentsQuery**
   - Returns all comments for task
   - Includes attachments
   - Threaded (parent-child relationships)
   - Returns List<TaskCommentDto>

8. **GetTaskStatisticsQuery**
   - Returns statistics for user
   - Total tasks created/assigned
   - Completed tasks count
   - Average completion time
   - Average rating received
   - Returns TaskStatisticsDto

### Master-SubUser Queries

9. **GetMasterUserTasksQuery** ⭐
   - SubUser views MasterUser's tasks
   - **Validation**: SubUser must have CanViewTasks permission for MasterUser
   - Returns tasks created by MasterUser
   - Filtered based on SubUser permissions
   - Returns List<TaskDto>

10. **GetSubUserTaskActivityQuery** ⭐
    - MasterUser views SubUser's activity on their tasks
    - Returns tasks where SubUser commented/updated
    - Returns List<TaskDto> with activity info

---

## Business Rules

### Task Creation & Ownership

1. **Creation**:
   - Only users with completed profile (UserEntity exists) can create tasks
   - Creator automatically becomes task owner
   - Task can be created as draft (no assignee) or assigned immediately
   - Title is required (max 200 chars)
   - Description is optional (max 5000 chars)

2. **Assignment**:
   - Task can have **ONE assignee at a time**
   - Can assign to any user with completed profile
   - Cannot assign to yourself (creator assigns to others)
   - Assignment creates TaskAssignment record
   - Status changes to Assigned when assigned

### Reassignment Rules ⭐

1. **Who can reassign**:
   - **Current assignee** can reassign IF:
     - AllowReassignment = true (set by creator)
     - Can reassign back to creator
     - Can reassign to any other user
   - **Creator** can always reassign (override AllowReassignment)

2. **Reassignment validation**:
   - Cannot reassign completed/cancelled tasks
   - Target user must have completed profile
   - Cannot reassign to current assignee (no-op)
   - Marks previous assignment as inactive
   - Creates new assignment record
   - Resets Status to Assigned

### Status Workflow

Valid status transitions:
```
Draft → Assigned (when first assigned)
Assigned → InProgress (assignee starts work)
Assigned → Cancelled (creator cancels)
InProgress → Blocked (assignee marks blocked)
InProgress → UnderReview (assignee marks complete, review required)
InProgress → Completed (assignee marks complete, no review required)
Blocked → InProgress (assignee unblocks)
Blocked → Cancelled (creator cancels)
UnderReview → Completed (creator reviews and approves)
UnderReview → InProgress (creator rejects, needs rework)
```

**Invalid transitions** (blocked):
- Completed → Any (cannot change completed task)
- Cancelled → Any (cannot resume cancelled task)

### Review & Completion ⭐

1. **RequiresReview = true**:
   - Assignee marks complete → Status = UnderReview
   - Creator must review before completion
   - Creator provides rating (1-5 stars) and comment
   - After review → Status = Completed

2. **RequiresReview = false**:
   - Assignee marks complete → Status = Completed (direct)
   - No review step
   - Optional: Assignee can provide completion notes

3. **Rating system**:
   - Stars: 1-5 (integer)
   - Optional review comment (max 2000 chars)
   - Only creator can review
   - Review is permanent (cannot change)

### Comments & Attachments

1. **Who can comment**:
   - Task creator (always)
   - Current assignee (always)
   - Previous assignees (always)
   - Master-SubUser: SubUser if CanViewTasks permission on MasterUser's task

2. **Attachments**:
   - Supported types: Images (jpg, png, gif), Videos (mp4, mov), Documents (pdf, docx, xlsx)
   - Max file size: 50MB per attachment
   - Max 10 attachments per comment
   - Stored with metadata: filename, size, type, URL
   - Thumbnails generated for images/videos

3. **Threading**:
   - Comments can have replies (ParentCommentId)
   - One level of nesting only (no deep threading)
   - Replies displayed under parent comment

### Time Tracking & History

1. **Time tracking**:
   - EstimatedHours: Set by creator during creation
   - ActualHours: Calculated from TaskHistory (sum of HoursTaken)
   - HoursTaken recorded for each status change
   - Assignee can log hours when changing status

2. **History tracking** (TaskHistory):
   - Every action creates history entry
   - Actions: Created, Assigned, Reassigned, StatusChanged, Completed, Cancelled, CommentAdded, etc.
   - Who, what, when, why (notes)
   - Previous/new values stored as JSON
   - Immutable (no edit/delete)

### Master-SubUser Integration ⭐

1. **SubUser viewing MasterUser's tasks**:
   - Requires CanViewTasks permission
   - Can view task details, comments, history
   - Cannot modify unless also granted CanUpdateTasks

2. **SubUser updating MasterUser's tasks**:
   - Requires CanUpdateTasks permission
   - Can change status, add comments
   - Cannot reassign (unless they are assignee)
   - Cannot cancel (only creator can cancel)

3. **SubUser creating tasks for MasterUser**:
   - Requires CanCreateTasks permission
   - Task.CreatedBy = SubUser.Id
   - Task appears in MasterUser's "my tasks" view
   - MasterUser can manage like normal task

4. **SubUser deleting MasterUser's tasks**:
   - Requires CanDeleteTasks permission
   - Only draft tasks can be deleted
   - Creates audit trail

### Authorization Rules

1. **Task creator** can:
   - Update task details
   - Assign/reassign to anyone
   - Cancel task
   - Review completed task (if RequiresReview = true)
   - View all task details

2. **Current assignee** can:
   - Change status (InProgress, Blocked, UnderReview, Completed)
   - Mark task complete
   - Add comments
   - Reassign (if AllowReassignment = true OR is creator)
   - View task details

3. **Other users** can:
   - View task (if shared or they have permission)
   - Add comments (if authorized)
   - View history/comments

---

## Domain Events

### Task Events
- `TaskCreatedDomainEvent` - Task created
- `TaskAssignedDomainEvent` - Task assigned to user
- `TaskReassignedDomainEvent` - Task reassigned
- `TaskStatusChangedDomainEvent` - Status changed
- `TaskStartedDomainEvent` - Assignee started work
- `TaskBlockedDomainEvent` - Task blocked
- `TaskUnblockedDomainEvent` - Task unblocked
- `TaskMarkedCompleteDomainEvent` - Assignee marked complete
- `TaskReviewedDomainEvent` - Creator reviewed task
- `TaskCompletedDomainEvent` - Task completed
- `TaskCancelledDomainEvent` - Task cancelled
- `TaskCommentAddedDomainEvent` - Comment added
- `TaskAttachmentAddedDomainEvent` - Attachment added
- `TaskDueDateChangedDomainEvent` - Due date changed
- `TaskPriorityChangedDomainEvent` - Priority changed

---

## User Flows

### Flow 1: Create and Assign Task

1. User A (creator, profile completed) creates task
2. System creates Task entity (Status = Draft, CreatedBy = A)
3. System creates TaskHistory (Action = Created)
4. User A assigns to User B via `AssignTaskCommand`
5. System validates User B has completed profile
6. System creates TaskAssignment (AssignedTo = B, AssignedBy = A, IsActive = true)
7. System updates Task (AssignedTo = B, Status = Assigned)
8. System creates TaskHistory (Action = Assigned)
9. System sends notification to User B
10. User B sees task in "Assigned to me" list

### Flow 2: Assignee Works and Completes Task (With Review)

1. User B (assignee) starts task via `StartTaskCommand`
2. System updates Status: Assigned → InProgress
3. System records StartedAt in TaskAssignment
4. System creates TaskHistory (Action = Started)
5. User B works on task (adds comments, logs progress)
6. User B marks complete via `MarkTaskCompleteCommand` (RequiresReview = true)
7. System updates Status: InProgress → UnderReview
8. System records hours spent in TaskHistory
9. System creates TaskHistory (Action = MarkedComplete)
10. System sends notification to User A (creator)
11. User A reviews via `ReviewTaskCommand`
12. User A provides rating (4 stars) and review comment
13. System updates Status: UnderReview → Completed
14. System sets CompletedAt, ReviewedAt, ReviewRating, ReviewComment
15. System creates TaskHistory (Action = Reviewed)
16. System sends notification to User B

### Flow 3: Assignee Reassigns Task

1. User B (current assignee) decides to reassign task
2. User B checks: AllowReassignment = true? YES
3. User B reassigns to User C via `ReassignTaskCommand`
4. System validates User C has completed profile
5. System marks current assignment (B) as inactive (IsActive = false, UnassignedAt = now)
6. System creates new TaskAssignment (AssignedTo = C, AssignedBy = B, AssignmentType = Reassignment)
7. System updates Task (AssignedTo = C, Status = Assigned)
8. System creates TaskHistory (Action = Reassigned, notes = "Reassigned from B to C")
9. System sends notification to User C
10. User C sees task in "Assigned to me" list
11. User B's assignment history preserved in TaskAssignment table

### Flow 4: Creator Cancels Task

1. User A (creator) decides to cancel task
2. User A cancels via `CancelTaskCommand` with reason
3. System updates Task (Status = Cancelled, CancelledAt = now, CancellationReason = reason)
4. System creates TaskHistory (Action = Cancelled)
5. System sends notification to current assignee (if any)
6. Task no longer appears in active tasks

### Flow 5: Multiple Users Comment with Attachments

1. User A (creator) adds comment with image attachment
2. System uploads image to storage (AWS S3, Azure Blob, etc.)
3. System generates thumbnail for image
4. System creates TaskComment with CommentAttachment
5. System creates TaskHistory (Action = CommentAdded)
6. User B (assignee) adds reply to User A's comment
7. System creates TaskComment with ParentCommentId = A's comment ID
8. User C (SubUser with CanViewTasks permission) adds comment
9. System validates User C has permission
10. System creates TaskComment
11. All comments displayed in task detail view with threading

### Flow 6: SubUser Manages MasterUser's Task

1. User A (MasterUser, profile completed) creates task
2. User D (SubUser, has CanUpdateTasks permission for A) views A's tasks
3. System validates: User D has CanViewTasks permission for A? YES
4. System shows A's tasks to User D
5. User D selects task and views details
6. User D changes status to InProgress via `ChangeTaskStatusCommand`
7. System validates: User D has CanUpdateTasks permission for A? YES
8. System updates task status
9. System creates TaskHistory (PerformedBy = D)
10. User D adds comment
11. User A (MasterUser) sees User D's activity on their task

### Flow 7: Task Completion Without Review

1. User A creates task with RequiresReview = false
2. User A assigns to User B
3. User B starts and completes work
4. User B marks complete via `MarkTaskCompleteCommand`
5. System updates Status: InProgress → Completed (direct, no review)
6. System sets CompletedAt
7. System creates TaskHistory (Action = Completed)
8. System sends notification to User A
9. Task appears in "Completed tasks" for both users

---

## Implementation Notes

1. **Task Entity**:
   - Aggregate root with collections: Assignments, Comments, History
   - Status workflow enforced in domain logic
   - Computed property: ActualHours (sum from History)

2. **TaskAssignment Entity**:
   - Tracks full assignment history
   - IsActive flag for current assignment
   - One active assignment per task at a time

3. **TaskComment Entity**:
   - Owned collection: CommentAttachment (value objects)
   - ParentCommentId for threading
   - No edit/delete (immutable)

4. **TaskHistory Entity**:
   - Complete audit trail
   - Every command creates history entry
   - PreviousValue/NewValue stored as JSON for flexibility
   - HoursTaken tracked per status change

5. **File Storage**:
   - Attachments stored in cloud storage (AWS S3, Azure Blob)
   - FileUrl points to storage location
   - ThumbnailUrl for images/videos
   - Max file size validation (50MB)
   - Supported types validation

6. **Notifications**:
   - Email notifications for: Assignment, Reassignment, Completion, Review, Cancellation
   - In-app notifications (optional)
   - Notification service integration

7. **Master-SubUser Integration**:
   - Permission checks in queries and commands
   - SubUser can view/manage tasks based on granted permissions
   - TaskHistory tracks SubUser actions on MasterUser tasks

8. **Performance Optimization**:
   - Pagination for task lists
   - Lazy loading for assignments/comments/history
   - Indexes on: CreatedBy, AssignedTo, Status, CreatedAt, DueDate

## Status

**Domain Layer**: ❌ To be generated

**Application Layer**: ❌ To be generated

**Infrastructure Layer**: ❌ To be generated

**API Layer**: ❌ To be generated

**Tests**: ❌ To be generated
