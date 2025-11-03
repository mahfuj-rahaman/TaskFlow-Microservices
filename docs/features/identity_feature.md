# Identity Feature Specification

**Service**: Identity
**Version**: 1.0.0
**Created**: 2025-11-03
**Status**: Ready for Generation

---

## Overview

### Purpose
Complete authentication and authorization system handling user identity, JWT token management, password security, email verification, and role-based access control. This is a production-ready identity provider compatible with TaskFlow microservices architecture.

### Scope
- User registration and authentication
- JWT access token + refresh token management
- Password hashing (BCrypt) and validation
- Email confirmation workflow
- Password reset functionality
- Account lockout after failed attempts
- Role-based access control (RBAC)
- Two-factor authentication (2FA) support
- Session management
- Security audit logging

---

## Domain Model

### Aggregate Root: Identity

#### Properties

| Property | Type | Required | Default | Constraints |
|----------|------|----------|---------|-------------|
| Id | Guid | Yes | NewGuid() | Primary Key |
| Username | string | Yes | - | Unique, 3-50 chars, alphanumeric + .-_ |
| Email | string | Yes | - | Unique, valid email format |
| PasswordHash | string | Yes | - | BCrypt hash, 500 chars max |
| EmailConfirmed | bool | Yes | false | - |
| EmailConfirmationToken | string? | No | null | 500 chars max |
| EmailConfirmationTokenExpiresAt | DateTime? | No | null | - |
| PasswordResetToken | string? | No | null | 500 chars max |
| PasswordResetTokenExpiresAt | DateTime? | No | null | - |
| Status | IdentityStatus | Yes | Active | Enum: Active, Inactive, Locked, Suspended |
| FailedLoginAttempts | int | Yes | 0 | 0-999 |
| LockoutEndAt | DateTime? | No | null | - |
| LastLoginAt | DateTime? | No | null | - |
| LastLoginIp | string? | No | null | 50 chars max |
| Roles | List\<string\> | Yes | \["User"\] | - |
| Permissions | List\<string\> | Yes | \[\] | Custom permissions |
| TwoFactorEnabled | bool | Yes | false | - |
| TwoFactorSecret | string? | No | null | 100 chars max |
| CreatedAt | DateTime | Yes | UtcNow | - |
| UpdatedAt | DateTime? | No | null | - |

#### Value Objects

**RefreshToken** (Owned Collection in Identity)
- Token (string, unique, 500 chars)
- ExpiresAt (DateTime)
- CreatedAt (DateTime)
- CreatedByIp (string)
- RevokedAt (DateTime?)
- RevokedByIp (string?)
- ReplacedByToken (string?)
- IsExpired (computed property)
- IsActive (computed property)

---

## Business Rules

### Registration Rules
1. Username must be unique (case-insensitive)
2. Username must be 3-50 characters long
3. Username can only contain alphanumeric characters, dots, underscores, and hyphens
4. Email must be unique (case-insensitive)
5. Email must be valid format
6. Password must be at least 8 characters
7. Password must contain: uppercase, lowercase, digit, and special character
8. Password is hashed with BCrypt (work factor 12)

### Authentication Rules
9. User must have EmailConfirmed = true to login
10. User Status must be Active to login
11. Account is locked for 30 minutes after 5 consecutive failed login attempts
12. Failed login attempts reset to 0 on successful login
13. LastLoginAt and LastLoginIp are updated on successful login

### Token Rules
14. JWT access tokens expire in 15 minutes
15. Refresh tokens expire in 7 days
16. Maximum 5 active refresh tokens per user (oldest revoked when limit exceeded)
17. All refresh tokens are revoked on password change
18. All refresh tokens are revoked when account is locked or suspended

### Email Confirmation Rules
19. Email confirmation token expires in 24 hours
20. New token is generated on re-request
21. Old tokens are invalidated when new token is generated

### Password Reset Rules
22. Password reset token expires in 1 hour
23. Token can only be used once
24. Password reset requires valid, non-expired token
25. Password history: cannot reuse last 3 passwords (future enhancement)

### Security Rules
26. All tokens are cryptographically secure random strings
27. IP address is tracked for security audit
28. Failed login attempts are logged
29. Account lockout events are logged and trigger notifications
30. Password changes trigger email notification

---

## Operations (CQRS)

### Commands (Write)

1. **RegisterIdentity**
   - Input: Username, Email, Password, ConfirmPassword
   - Output: IdentityId, EmailConfirmationToken
   - Side Effects: Raises IdentityCreatedEvent, sends confirmation email

2. **LoginIdentity**
   - Input: UsernameOrEmail, Password, IpAddress
   - Output: JwtToken, RefreshToken
   - Side Effects: Records login, resets failed attempts

3. **RefreshToken**
   - Input: RefreshToken, IpAddress
   - Output: NewJwtToken, NewRefreshToken
   - Side Effects: Revokes old token, issues new token

4. **LogoutIdentity**
   - Input: RefreshToken
   - Output: Success
   - Side Effects: Revokes refresh token

5. **ConfirmEmail**
   - Input: IdentityId, ConfirmationToken
   - Output: Success
   - Side Effects: Sets EmailConfirmed = true

6. **RequestPasswordReset**
   - Input: Email
   - Output: Success (always for security)
   - Side Effects: Generates token, sends email

7. **ResetPassword**
   - Input: Email, ResetToken, NewPassword
   - Output: Success
   - Side Effects: Changes password, revokes all refresh tokens

8. **ChangePassword**
   - Input: IdentityId, CurrentPassword, NewPassword
   - Output: Success
   - Side Effects: Changes password, revokes all refresh tokens

9. **AddRole**
   - Input: IdentityId, RoleName
   - Output: Success
   - Side Effects: Adds role to Roles collection

10. **RemoveRole**
    - Input: IdentityId, RoleName
    - Output: Success
    - Side Effects: Removes role from Roles collection

11. **LockAccount**
    - Input: IdentityId, Reason
    - Output: Success
    - Side Effects: Sets Status = Locked, revokes tokens

12. **UnlockAccount**
    - Input: IdentityId
    - Output: Success
    - Side Effects: Sets Status = Active, clears lockout

### Queries (Read)

1. **GetIdentityById**
   - Input: IdentityId
   - Output: IdentityDto

2. **GetIdentityByUsername**
   - Input: Username
   - Output: IdentityDto

3. **GetIdentityByEmail**
   - Input: Email
   - Output: IdentityDto

4. **GetAllIdentities**
   - Input: PageNumber, PageSize, SearchTerm, Status
   - Output: PagedList\<IdentityDto\>

5. **ValidateJwtToken**
   - Input: JwtToken
   - Output: IdentityDto or null

6. **CheckUsernameAvailable**
   - Input: Username
   - Output: bool

7. **CheckEmailAvailable**
   - Input: Email
   - Output: bool

---

## API Endpoints

### Authentication Controller

| Method | Endpoint | Description | Auth | Rate Limit |
|--------|----------|-------------|------|------------|
| POST | /api/auth/register | Register new user | Anonymous | 3/min |
| POST | /api/auth/login | Login user | Anonymous | 5/min |
| POST | /api/auth/refresh-token | Refresh JWT | Anonymous | 10/min |
| POST | /api/auth/logout | Logout user | Bearer | - |
| POST | /api/auth/confirm-email | Confirm email | Anonymous | 10/min |
| POST | /api/auth/forgot-password | Request reset | Anonymous | 3/min |
| POST | /api/auth/reset-password | Reset password | Anonymous | 3/min |
| POST | /api/auth/change-password | Change password | Bearer | 5/min |
| GET | /api/auth/me | Get current user | Bearer | - |

### Identity Management Controller (Admin)

| Method | Endpoint | Description | Auth | Rate Limit |
|--------|----------|-------------|------|------------|
| GET | /api/identities | Get all identities | Admin | - |
| GET | /api/identities/{id} | Get identity by ID | Admin | - |
| GET | /api/identities/username/{username} | Get by username | Admin | - |
| GET | /api/identities/email/{email} | Get by email | Admin | - |
| POST | /api/identities/{id}/roles | Add role | Admin | - |
| DELETE | /api/identities/{id}/roles/{role} | Remove role | Admin | - |
| POST | /api/identities/{id}/lock | Lock account | Admin | - |
| POST | /api/identities/{id}/unlock | Unlock account | Admin | - |

---

## Domain Events

1. **IdentityCreatedDomainEvent**
   - IdentityId, Username, Email, CreatedAt

2. **IdentityUpdatedDomainEvent**
   - IdentityId, UpdatedAt

3. **IdentityDeletedDomainEvent**
   - IdentityId, DeletedAt

4. **LoginSucceededDomainEvent**
   - IdentityId, LoginAt, IpAddress

5. **LoginFailedDomainEvent**
   - IdentityId, FailedAt, IpAddress, Reason

6. **AccountLockedDomainEvent**
   - IdentityId, LockedAt, Reason, LockoutEndAt

7. **AccountUnlockedDomainEvent**
   - IdentityId, UnlockedAt

8. **PasswordChangedDomainEvent**
   - IdentityId, ChangedAt

9. **EmailConfirmedDomainEvent**
   - IdentityId, ConfirmedAt

10. **RoleAddedDomainEvent**
    - IdentityId, RoleName, AddedAt

11. **RoleRemovedDomainEvent**
    - IdentityId, RoleName, RemovedAt

---

## Integration Events (Message Bus)

Events published to RabbitMQ for other services:

1. **IdentityCreatedIntegrationEvent** → User Service, Notification Service
2. **LoginSucceededIntegrationEvent** → Audit Service
3. **LoginFailedIntegrationEvent** → Audit Service, Security Service
4. **AccountLockedIntegrationEvent** → Security Service, Notification Service
5. **PasswordChangedIntegrationEvent** → Notification Service

---

## Infrastructure Dependencies

### External Services
- Email Service (SendGrid, AWS SES, or SMTP)
- Redis Cache (token blacklisting, rate limiting)
- PostgreSQL Database

### BuildingBlocks Used
- TaskFlow.BuildingBlocks.Common (Domain, Results, Exceptions)
- TaskFlow.BuildingBlocks.CQRS (Commands, Queries, Handlers)
- TaskFlow.BuildingBlocks.Caching (Response caching)
- TaskFlow.BuildingBlocks.Messaging (Event publishing)
- TaskFlow.BuildingBlocks.EventBus (Integration events)

### NuGet Packages
- BCrypt.Net-Next (Password hashing)
- System.IdentityModel.Tokens.Jwt (JWT generation/validation)
- FluentValidation (Input validation)
- MediatR (CQRS mediator)
- Mapster (DTO mapping)

---

## Security Considerations

### Password Security
- BCrypt with work factor 12
- Minimum 8 characters
- Complexity requirements enforced
- Never stored in plain text
- Never logged or returned in responses

### Token Security
- Cryptographically secure random tokens
- Short-lived access tokens (15 min)
- Longer-lived refresh tokens (7 days)
- Token rotation on refresh
- Token revocation support
- IP address tracking

### API Security
- HTTPS only in production
- Rate limiting on auth endpoints
- CORS configured properly
- JWT Bearer authentication
- Authorization policies

### Audit & Compliance
- All authentication attempts logged
- Failed login tracking
- Account lockout logging
- IP address logging
- GDPR compliant (data export/deletion support)

---

## Database Schema

### Identities Table
```sql
CREATE TABLE Identities (
    Id UUID PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(256) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    EmailConfirmed BOOLEAN NOT NULL DEFAULT FALSE,
    EmailConfirmationToken VARCHAR(500),
    EmailConfirmationTokenExpiresAt TIMESTAMP,
    PasswordResetToken VARCHAR(500),
    PasswordResetTokenExpiresAt TIMESTAMP,
    Status INTEGER NOT NULL DEFAULT 1,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockoutEndAt TIMESTAMP,
    LastLoginAt TIMESTAMP,
    LastLoginIp VARCHAR(50),
    Roles JSONB NOT NULL DEFAULT '["User"]',
    Permissions JSONB NOT NULL DEFAULT '[]',
    TwoFactorEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    TwoFactorSecret VARCHAR(100),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,

    CONSTRAINT CK_Identities_Email CHECK (Email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT CK_Identities_Username CHECK (Username ~* '^[A-Za-z0-9._-]{3,50}$')
);

CREATE INDEX IX_Identities_Username ON Identities(LOWER(Username));
CREATE INDEX IX_Identities_Email ON Identities(LOWER(Email));
CREATE INDEX IX_Identities_EmailConfirmationToken ON Identities(EmailConfirmationToken) WHERE EmailConfirmationToken IS NOT NULL;
CREATE INDEX IX_Identities_PasswordResetToken ON Identities(PasswordResetToken) WHERE PasswordResetToken IS NOT NULL;
CREATE INDEX IX_Identities_Status ON Identities(Status);
```

### RefreshTokens Table (Owned by Identity)
```sql
CREATE TABLE RefreshTokens (
    Id UUID PRIMARY KEY,
    IdentityId UUID NOT NULL REFERENCES Identities(Id) ON DELETE CASCADE,
    Token VARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedByIp VARCHAR(50) NOT NULL,
    RevokedAt TIMESTAMP,
    RevokedByIp VARCHAR(50),
    ReplacedByToken VARCHAR(500),

    CONSTRAINT FK_RefreshTokens_Identity FOREIGN KEY (IdentityId) REFERENCES Identities(Id)
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_IdentityId ON RefreshTokens(IdentityId);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);
```

---

## Testing Strategy

### Unit Tests
- [ ] Domain entity validation
- [ ] Password hashing and verification
- [ ] Token generation and validation
- [ ] Account lockout logic
- [ ] Command handler business logic
- [ ] Query handler logic
- [ ] All validators

### Integration Tests
- [ ] Registration flow end-to-end
- [ ] Login flow with database
- [ ] Email confirmation flow
- [ ] Password reset flow
- [ ] Token refresh flow
- [ ] Account lockout behavior

### Security Tests
- [ ] SQL injection attempts
- [ ] Brute force login attempts
- [ ] Token tampering detection
- [ ] Expired token rejection
- [ ] Rate limiting enforcement

---

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "Issuer": "taskflow-identity",
    "Audience": "taskflow-api",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Security": {
    "PasswordPolicy": {
      "RequiredLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "AccountLockout": {
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 30
    },
    "Tokens": {
      "EmailConfirmationExpirationHours": 24,
      "PasswordResetExpirationHours": 1
    },
    "RefreshTokens": {
      "MaxActiveTokensPerUser": 5
    }
  },
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "your-api-key",
    "FromEmail": "noreply@taskflow.com",
    "FromName": "TaskFlow"
  }
}
```

---

## Implementation Checklist

### Phase 1: Domain Layer
- [ ] IdentityEntity
- [ ] RefreshToken value object
- [ ] IdentityStatus enum
- [ ] All domain events
- [ ] All domain exceptions

### Phase 2: Application Layer
- [ ] All DTOs
- [ ] All Commands with Handlers and Validators
- [ ] All Queries with Handlers
- [ ] IIdentityRepository interface
- [ ] ITokenService interface
- [ ] IPasswordHasher interface
- [ ] IEmailService interface

### Phase 3: Infrastructure Layer
- [ ] IdentityRepository implementation
- [ ] IdentityConfiguration (EF Core)
- [ ] JwtTokenService implementation
- [ ] BCryptPasswordHasher implementation
- [ ] EmailService implementation
- [ ] Database migrations

### Phase 4: API Layer
- [ ] AuthController
- [ ] IdentitiesController
- [ ] JWT authentication middleware
- [ ] Rate limiting middleware
- [ ] Error handling middleware
- [ ] Swagger documentation

### Phase 5: Testing
- [ ] Unit tests (Domain + Application)
- [ ] Integration tests (API)
- [ ] Security tests

### Phase 6: Deployment
- [ ] Docker configuration
- [ ] Kubernetes manifests
- [ ] CI/CD pipeline
- [ ] Monitoring and logging

---

## Next Steps

1. Run scaffolding script:
   ```bash
   ./scripts/scaffold-service.sh docs/features/identity_feature.md
   ```

2. Review generated code in `src/Services/Identity`

3. Customize business logic as needed

4. Run tests:
   ```bash
   dotnet test
   ```

5. Build and run:
   ```bash
   docker-compose up -d identity-service
   ```

6. Access Swagger UI: http://localhost:5000/api/identity/swagger

---

**Status**: Ready for scaffolding ✅
**Complexity**: High
**Estimated Development Time**: 40-60 hours (manual) vs 2 minutes (scaffolded)
**Dependencies**: All BuildingBlocks, API Gateway

