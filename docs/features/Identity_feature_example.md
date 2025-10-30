# Identity Feature Specification

**Service**: User
**Created**: 2025-10-30
**Status**: Draft

---

## 1. Overview

### Purpose
Manage user authentication and identity information including email, password, and authentication tokens. Supports user registration, login, password management, and session handling.

### Scope
This feature handles:
- User registration with email/password
- User authentication and login
- Password hashing and validation
- JWT token generation
- Password reset functionality
- Email verification
- Account lockout after failed attempts

---

## 2. Business Requirements

### Functional Requirements
- [x] User can register with email and password
- [x] User can login with credentials
- [x] System validates email format and uniqueness
- [x] System hashes passwords securely (BCrypt)
- [x] System generates JWT tokens on successful login
- [x] User can reset password via email
- [x] User account locks after 5 failed login attempts
- [x] System sends verification email on registration

### Non-Functional Requirements
- [x] Performance: Login must complete within 500ms
- [x] Security: Passwords must be hashed with BCrypt (cost factor 12)
- [x] Security: JWT tokens expire after 24 hours
- [x] Scalability: Support 10,000 concurrent users

---

## 3. Domain Model

### Aggregate Root: IdentityEntity

#### Properties
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| Email | string | Yes | User's email (unique) |
| PasswordHash | string | Yes | BCrypt hashed password |
| IsEmailVerified | bool | Yes | Email verification status |
| EmailVerificationToken | string? | No | Token for email verification |
| PasswordResetToken | string? | No | Token for password reset |
| PasswordResetTokenExpiry | DateTime? | No | Reset token expiration |
| FailedLoginAttempts | int | Yes | Count of failed logins |
| LockoutUntil | DateTime? | No | Account lockout expiry |
| LastLoginAt | DateTime? | No | Last successful login |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |

#### Business Rules
1. Email must be unique across all users
2. Email must be in valid format
3. Password must be at least 8 characters
4. Password must contain uppercase, lowercase, number, and special char
5. Account locks for 15 minutes after 5 failed login attempts
6. Email verification token expires after 24 hours
7. Password reset token expires after 1 hour
8. User cannot login if email is not verified
9. User cannot login if account is locked

#### Domain Events
- [x] IdentityCreated - When new identity is registered
- [x] LoginSucceeded - When user successfully logs in
- [x] LoginFailed - When login attempt fails
- [x] AccountLocked - When account is locked due to failed attempts
- [x] PasswordResetRequested - When user requests password reset
- [x] PasswordChanged - When password is successfully changed
- [x] EmailVerified - When email verification succeeds

---

## 4. Use Cases

### Commands (Write Operations)

1. **RegisterIdentity**
   - Input: Email, Password, ConfirmPassword
   - Output: IdentityId, EmailVerificationToken
   - Validation:
     - Email format valid
     - Email not already registered
     - Password meets complexity requirements
     - Passwords match

2. **LoginIdentity**
   - Input: Email, Password
   - Output: JWT Token, RefreshToken
   - Validation:
     - Email exists
     - Password correct
     - Email verified
     - Account not locked

3. **VerifyEmail**
   - Input: IdentityId, VerificationToken
   - Output: Success/Failure
   - Validation:
     - Token matches
     - Token not expired

4. **RequestPasswordReset**
   - Input: Email
   - Output: Success (always, for security)
   - Validation: Email format valid

5. **ResetPassword**
   - Input: Email, ResetToken, NewPassword
   - Output: Success/Failure
   - Validation:
     - Token valid and not expired
     - New password meets requirements

6. **ChangePassword**
   - Input: IdentityId, CurrentPassword, NewPassword
   - Output: Success/Failure
   - Validation:
     - Current password correct
     - New password meets requirements
     - New password different from old

### Queries (Read Operations)

1. **GetIdentityById**
   - Input: IdentityId
   - Output: IdentityDto

2. **GetIdentityByEmail**
   - Input: Email
   - Output: IdentityDto

3. **ValidateToken**
   - Input: JWT Token
   - Output: IdentityDto or null

---

## 5. API Endpoints

| Method | Endpoint | Description | Request | Response |
|--------|----------|-------------|---------|----------|
| POST | /api/v1/auth/register | Register new user | RegisterIdentityCommand | 201 Created |
| POST | /api/v1/auth/login | Login user | LoginIdentityCommand | 200 OK + JWT |
| POST | /api/v1/auth/verify-email | Verify email | VerifyEmailCommand | 200 OK |
| POST | /api/v1/auth/forgot-password | Request reset | RequestPasswordResetCommand | 200 OK |
| POST | /api/v1/auth/reset-password | Reset password | ResetPasswordCommand | 200 OK |
| POST | /api/v1/auth/change-password | Change password | ChangePasswordCommand | 200 OK |
| GET | /api/v1/auth/me | Get current user | - | 200 OK |
| POST | /api/v1/auth/refresh | Refresh token | RefreshTokenCommand | 200 OK |

---

## 6. Data Model

### Database Table: Identities

```sql
CREATE TABLE Identities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    EmailVerificationToken NVARCHAR(512) NULL,
    PasswordResetToken NVARCHAR(512) NULL,
    PasswordResetTokenExpiry DATETIME2 NULL,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockoutUntil DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT CK_Identities_Email CHECK (Email LIKE '%_@__%.__%')
);

CREATE INDEX IX_Identities_Email ON Identities(Email);
CREATE INDEX IX_Identities_EmailVerificationToken ON Identities(EmailVerificationToken) WHERE EmailVerificationToken IS NOT NULL;
CREATE INDEX IX_Identities_PasswordResetToken ON Identities(PasswordResetToken) WHERE PasswordResetToken IS NOT NULL;
```

### Indexes
- [x] Primary Key on Id
- [x] Unique Index on Email
- [x] Index on EmailVerificationToken (filtered)
- [x] Index on PasswordResetToken (filtered)

---

## 7. Security & Authorization

### Permissions Required
- Create Identity: Anonymous (public registration)
- Login: Anonymous
- VerifyEmail: Anonymous (with token)
- ResetPassword: Anonymous (with token)
- ChangePassword: Authenticated user (own account only)
- GetIdentityById: Admin only
- GetIdentityByEmail: Admin only

### Security Measures
- [x] Password hashing with BCrypt (cost factor 12)
- [x] JWT token signing with HMAC-SHA256
- [x] Token expiration (24 hours)
- [x] Refresh token rotation
- [x] Account lockout mechanism
- [x] Rate limiting on login endpoint (5 attempts per minute)
- [x] HTTPS required for all endpoints
- [x] CORS configured for allowed origins only

---

## 8. Integration Points

### Dependencies
- [x] Email Service (for verification and password reset emails)
- [x] JWT Token Service (for token generation and validation)
- [x] Hashing Service (BCrypt for password hashing)

### Events Published
- [x] IdentityCreatedEvent → User Service, Email Service
- [x] LoginSucceededEvent → Audit Service
- [x] LoginFailedEvent → Audit Service, Security Service
- [x] AccountLockedEvent → Security Service, Notification Service
- [x] PasswordChangedEvent → Audit Service, Email Service

### Events Consumed
- [x] UserDeletedEvent → Delete associated identity

---

## 9. Testing Strategy

### Unit Tests
- [x] IdentityEntity validation rules
- [x] Password hashing and verification
- [x] Token generation and validation
- [x] Account lockout logic
- [x] Command handler business logic
- [x] Query handler logic
- [x] Validators for all commands

### Integration Tests
- [x] Registration flow end-to-end
- [x] Login flow with database
- [x] Email verification flow
- [x] Password reset flow
- [x] Account lockout behavior
- [x] Token refresh flow

### Security Tests
- [x] SQL injection attempts
- [x] Brute force login attempts
- [x] Token tampering detection
- [x] Expired token rejection

---

## 10. Implementation Checklist

### Domain Layer
- [ ] IdentityEntity.cs
- [ ] IdentityStatus enum
- [ ] IdentityCreatedDomainEvent.cs
- [ ] LoginSucceededDomainEvent.cs
- [ ] LoginFailedDomainEvent.cs
- [ ] AccountLockedDomainEvent.cs
- [ ] PasswordChangedDomainEvent.cs
- [ ] IdentityDomainException.cs

### Application Layer
- [ ] IdentityDto.cs
- [ ] IIdentityRepository.cs
- [ ] ITokenService.cs
- [ ] IPasswordHasher.cs
- [ ] Commands:
  - [ ] RegisterIdentityCommand + Handler + Validator
  - [ ] LoginIdentityCommand + Handler + Validator
  - [ ] VerifyEmailCommand + Handler + Validator
  - [ ] RequestPasswordResetCommand + Handler + Validator
  - [ ] ResetPasswordCommand + Handler + Validator
  - [ ] ChangePasswordCommand + Handler + Validator
- [ ] Queries:
  - [ ] GetIdentityByIdQuery + Handler
  - [ ] GetIdentityByEmailQuery + Handler
  - [ ] ValidateTokenQuery + Handler
- [ ] IdentityMappingConfig.cs

### Infrastructure Layer
- [ ] IdentityRepository.cs
- [ ] IdentityConfiguration.cs (EF Core mapping)
- [ ] JwtTokenService.cs
- [ ] BCryptPasswordHasher.cs
- [ ] Database migration

### API Layer
- [ ] AuthController.cs
- [ ] JWT Authentication middleware configuration
- [ ] Rate limiting middleware
- [ ] API documentation (Swagger)

### Tests
- [ ] IdentityEntityTests.cs
- [ ] RegisterIdentityCommandHandlerTests.cs
- [ ] LoginIdentityCommandHandlerTests.cs
- [ ] PasswordHasherTests.cs
- [ ] TokenServiceTests.cs
- [ ] AuthControllerIntegrationTests.cs

---

## Next Steps

1. Review this specification with stakeholders
2. Run: `./scripts/generate-from-spec.sh Identity User`
3. Review generated code
4. Run unit tests
5. Create and apply database migration
6. Run integration tests
7. Deploy to dev environment
8. Perform security testing
9. Deploy to production
